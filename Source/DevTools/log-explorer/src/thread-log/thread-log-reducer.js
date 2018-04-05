import * as ThreadLog from './thread-log';
import * as Protocol from './thread-log-protocol';

//-------------------------------------------------------------------------------------------------------------------------------------------------------------

function normalizeTime(value) {
    if (typeof value === 'number') {
        return value;
    }
    if (typeof value === 'string') {
        return Date.parse(value);
    }
    throw new Error(`bad format (date/time): ${value}`);
}

//-------------------------------------------------------------------------------------------------------------------------------------------------------------

function normalizeBoolean(value) {
    if (typeof value === 'boolean') {
        return value;
    }
    if (value === 'true' || value === 'false') {
        return (value === 'true');
    }
    throw new Error(`bad format (boolean): ${value}`);
}

//-------------------------------------------------------------------------------------------------------------------------------------------------------------

export class ThreadLogReducer {

    constructor(onThreadCompleted) {
        this._onThreadCompleted = onThreadCompleted;

        this._opCodeHandlers = { };
        this._registerOpCodeHandlers();

        this._dictionary = new Protocol.LogDictionary();
        this._threadByKey = { };
        this._openThread = null;
        this._openMessage = null;
        this._openKvpKey = null;
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    push(command) {
        if (typeof command !== 'object' || typeof command.op !== 'number') {
            throw new Error('bad argument (command): must be a valid command object');
        }

        const handler = this._opCodeHandlers[command.op];

        if (typeof handler !== 'function') {
            throw new Error(`bad argument (command): opcode ${command.op} is not supported`);
        }

        handler(command);
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    get minTime() {
        return this._minTime;
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    get maxTime() {
        return this._maxTime;
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    _registerOpCodeHandlers() {
        this._opCodeHandlers[Protocol.OPCODE_DICTIONARY_ENTRY] = c => this._handleDictionaryEntry(c);
        this._opCodeHandlers[Protocol.OPCODE_THREAD_START] = c => this._handleThreadStart(c);
        this._opCodeHandlers[Protocol.OPCODE_MESSAGE_START] = c => this._handleMessageStart(c);
        this._opCodeHandlers[Protocol.OPCODE_CLOSE_ACTIVITY_START] = c => this._handleCloseActivityStart(c);
        this._opCodeHandlers[Protocol.OPCODE_KVP_KEY] = c => this._handleKvpKey(c);
        this._opCodeHandlers[Protocol.OPCODE_KVP_VALUE_NULL] = c => this._handleKvpValueNull(c);
        this._opCodeHandlers[Protocol.OPCODE_KVP_VALUE_NUMBER] = c => this._handleKvpValueNumber(c);
        this._opCodeHandlers[Protocol.OPCODE_KVP_VALUE_BOOLEAN] = c => this._handleKvpValueBoolean(c);
        this._opCodeHandlers[Protocol.OPCODE_KVP_VALUE_STRING] = c => this._handleKvpValueString(c);
        this._opCodeHandlers[Protocol.OPCODE_EXCEPTION] = c => this._handleException(c);
        this._opCodeHandlers[Protocol.OPCODE_MESSAGE_END] = c => this._handleMessageEnd(c);
        this._opCodeHandlers[Protocol.OPCODE_CLOSE_ACTIVITY_END] = c => this._handleCloseActivityEnd(c);
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    _handleDictionaryEntry(command) {
        this._dictionary.setString(command.k, command.v);
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    _handleThreadStart(command) {
        this._validateNoMessageOpen(command.op);
        
        command.t0 = normalizeTime(command.t0);
        this._threadByKey[command.K] = new ThreadLog.Thread(command);
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    _handleMessageStart(command) {
        this._validateNoMessageOpen(command.op);

        const thread = this._getThreadByKey(command.K);
        const newMessage = new ThreadLog.Message(command, thread.currentActivity);
        
        thread.append(newMessage);
        
        this._openThread = thread;
        this._openMessage = newMessage;
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    _handleKvpKey(command) {
        this._validateOpenMessage(command.op);
        this._validateNoKeyOpen(command.op);

        this._openKvpKey = command.k;
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    _handleKvpValueNull(command) {
        this._validateOpenMessage(command.op);
        this._validateOpenKey(command.op);

        this._openMessage.addValue(this._openKvpKey, null, null);
        this._openKvpKey = null;
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    _handleKvpValueNumber(command) {
        this._validateOpenMessage(command.op);
        this._validateOpenKey(command.op);

        this._openMessage.addValue(this._openKvpKey, command.v, 'number');
        this._openKvpKey = null;
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    _handleKvpValueString(command) {
        this._validateOpenMessage(command.op);
        this._validateOpenKey(command.op);

        this._openMessage.addValue(this._openKvpKey, command.v, 'string');
        this._openKvpKey = null;
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    _handleKvpValueBoolean(command) {
        this._validateOpenMessage(command.op);
        this._validateOpenKey(command.op);

        this._openMessage.addValue(this._openKvpKey, command.v, 'boolean');
        this._openKvpKey = null;
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    _handleException(command) {
        this._validateOpenMessage(command.op);
        this._validateNoKeyOpen(command.op);

        this._openMessage.setException(command.t, command.m);
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    _handleMessageEnd(command) {
        this._validateOpenMessage(command.op);
    
        this._openThread = null;
        this._openMessage = null;
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    _handleCloseActivityStart(command) {
        this._validateNoMessageOpen(command.op);

        const thread = this._getThreadByKey(command.K);
        const activity = thread.currentActivity;

        activity.setEndTime(command.t);

        this._openThread = thread;
        this._openMessage = activity;
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    _handleCloseActivityEnd(command) {
        this._validateOpenActivity(command.op);

        const thread = this._openThread;

        thread.closeActivity();

        this._openThread = null;
        this._openMessage = null;

        if (thread.isClosed) {
            this._onThreadCompleted(thread);
        }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    // _includeTime(time) {
    //     if (this._minTime === null || time < this._minTime) {
    //         this._minTime = time;
    //     }
    //     if (this._maxTime === null || time > this._maxTime) {
    //         this._maxTime = time;
    //     }
    //     return time;
    // }
    
    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    _getThreadByKey(key) {
        const thread = this._threadByKey[key];
        if (!thread) {
            throw new Error(`bad format (thread not found): key=${key}`);
        }
        return thread;
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    _validateNoMessageOpen(opCode) {
        if (this._openThread || this._openMessage) {
            throw new Error(`bad format: opcode ${opCode} while a message is open`);
        }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    _validateOpenMessage(opCode) {
        if (!this._openThread || !this._openMessage) {
            throw new Error(`bad format: opcode ${opCode} while there is no open message`);
        }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    _validateOpenActivity(opCode) {
        this._validateOpenMessage(opCode);
        if (!this._openMessage.isActivity) {
            throw new Error(`bad format: opcode ${opCode} while open message is not an activity`);
        }
    }   

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    _validateOpenKey(opCode) {
        if (this._openKvpKey === null) {
            throw new Error(`bad format: opcode ${opCode} while no KVP is open`);
        }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    _validateNoKeyOpen(opCode) {
        if (this._openKvpKey !== null) {
            throw new Error(`bad format: opcode ${opCode} while KVP key is open`);
        }
    }
};
