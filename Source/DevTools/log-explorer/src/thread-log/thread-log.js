import * as Protocol from './thread-log-protocol';

//-------------------------------------------------------------------------------------------------------------------------------------------------------------

export class Thread {
    constructor(command) {
        if (!command || command.op !== Protocol.OPCODE_THREAD_START) {
            throw new Error('bad argument (command): must be OPCODE_THREAD_START command');
        }

        this._key = command.K;
        this._processId = command.pid;
        this._processName = command.pn;
        this._threadId = command.tid;
        this._threadName = command.tn;
        this._startedAt = command.t0;

        this._rootActivity = null;
        this._currentActivity = null;
        this._isClosed = false;
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    append(message) {
        if (!(message instanceof Message)) {
            throw new Error('bad argument (message): must be a Message object');
        }
        if (this._isClosed) {
            throw new Error('bad operation: cannot append because root activity was closed');
        }

        if (this._currentActivity) {
            this._currentActivity.appendChild(message);

            if (message.isActivity) {
                this._currentActivity = message;
            }
        } else {
            if (!message.isActivity) {
                throw new Error('bad operation: first message must be root activity');
            }
            this._rootActivity = message;
            this._currentActivity = message;
        }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    closeActivity() {
        if (!this._currentActivity) {
            throw new Error('bad operation: no current activity');
        }
        
        this._currentActivity = this._currentActivity.parent;
        if (!this._currentActivity) {
            this._isClosed = true;
        }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    get key() { return this._key; }
    get startedAt() { return this._startedAt; }
    get processId() { return this._processId; }
    get processName() { return this._processName; }
    get threadId() { return this._threadId; }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    get threadName() { 
        return this._threadName; 
    }
    set threadName(value) {
        this._threadName = value;
    }
    
    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    get rootActivity() {
        return this._rootActivity;
    }
    get currentActivity() {
        return this._currentActivity;
    }
    get isClosed() {
        return this._isClosed;
    }
}

//-------------------------------------------------------------------------------------------------------------------------------------------------------------

export class Message {
    constructor(command, parent = null) {
        if (!command || command.op !== Protocol.OPCODE_MESSAGE_START) {
            throw new Error('bad argument (command): must be OPCODE_MESSAGE_START command');
        }
        if (parent && !(parent instanceof Message)) {
            throw new Error('bad argument (parent): must be a Message object');
        }

        this._parent = parent;
        this._time = command.t;
        this._level = command.l;
        this._flags = command.f;
        this._componentId = command.c;
        this._messageId = command.m;
        this._duration = undefined;
        this._exception = null;

        this._values = {};
        this._firstChild = null;
        this._lastChild = null;
        this._nextSibling = null;
        this._prevSibling = null;
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    addValue(key, value, type) {
        this._values[key] = { value, type };
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    getValue(key) {
        return this._values[key];
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    appendChild(message) {
        if (!(message instanceof Message)) {
            throw new Error('bad argument (message): must be a LogNode object');
        }

        message._parent = this;

        if (this._lastChild) {
            message._prevSibling = this._lastChild;
            this._lastChild._nextSibling = message;
            this._lastChild = message;
        } else {
            this._firstChild = message;
            this._lastChild = message;
        }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    setException(type, message) {
        this._exception = {
            type,
            message
        };
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    setEndTime(endTime) {
        this._duration = endTime - this._time;
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    get parent() { return this._parent; }
    get time() { return this._time; }
    get level() { return this._level; }
    get flags() { return this._flags; }
    get componentId() { return this._componentId; }
    get messageId() { return this._messageId; }
    get values() { return this._values; }
    get duration() { return this._duration; }
    get exception() { return this._exception; }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    get parent() { return this._parent; }
    get firstChild() { return this._firstChild; }
    get lastChild() { return this._lastChild; }
    get prevSibling() { return this._prevSibling; }
    get nextSibling() { return this._nextSibling; }
    
    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    get isActivity() {
        return ((this._flags & Protocol.LOG_FLAGS_ACTIVITY) != 0);
    }
    get isMessage() {
        return ((this._flags & Protocol.LOG_FLAGS_MESSAGE) != 0);
    }
    get isBoundary() {
        return ((this._flags & Protocol.LOG_FLAGS_BOUNDARY) != 0);
    }
    get isIncoming() {
        return ((this._flags & Protocol.LOG_FLAGS_INCOMING) != 0);
    }
    get isOutgoing() {
        return ((this._flags & Protocol.LOG_FLAGS_OUTGOING) != 0);
    }
}

//-------------------------------------------------------------------------------------------------------------------------------------------------------------

