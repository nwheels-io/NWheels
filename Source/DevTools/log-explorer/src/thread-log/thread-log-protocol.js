export const OPCODE_DICTIONARY_ENTRY     = 1;
export const OPCODE_THREAD_START         = 2;
export const OPCODE_MESSAGE_START        = 3;
export const OPCODE_CLOSE_ACTIVITY_START = 4;
export const OPCODE_KVP_KEY              = 5;
export const OPCODE_KVP_VALUE_NULL       = 6;
export const OPCODE_KVP_VALUE_STRING     = 7;
export const OPCODE_KVP_VALUE_NUMBER     = 8;
export const OPCODE_KVP_VALUE_BOOLEAN    = 9;
export const OPCODE_EXCEPTION            = 10;
export const OPCODE_EXCEPTION_REF        = 11;
export const OPCODE_MESSAGE_END          = 12;
export const OPCODE_CLOSE_ACTIVITY_END   = 13;
export const OPCODE_CHANGE_THREAD_NAME   = 14;
export const OPCODE_PARSER_STATS         = 99;

export const LOG_LEVEL_DEBUG = 0;
export const LOG_LEVEL_VERBOSE = 1;
export const LOG_LEVEL_INFO = 2;
export const LOG_LEVEL_WARNING = 3;
export const LOG_LEVEL_ERROR = 4;
export const LOG_LEVEL_CRITICAL = 5;
export const LOG_LEVEL_DISABLED = 100;

export const LOG_FLAGS_NONE = 0;
export const LOG_FLAGS_ACTIVITY = 0x01;
export const LOG_FLAGS_MESSAGE = 0x02;
export const LOG_FLAGS_METADATA = 0x04;
export const LOG_FLAGS_BOUNDARY = 0x08;
export const LOG_FLAGS_INCOMING = 0x10;
export const LOG_FLAGS_OUTGOING = 0x20;
export const LOG_FLAGS_ACITVITY_CONTEXT = 0x40;
export const LOG_FLAGS_RPC_INVOKE = 0x80;

export const LOG_FLAGS_BOUNDARY_IN = LOG_FLAGS_INCOMING | LOG_FLAGS_BOUNDARY;
export const LOG_FLAGS_BOUNDARY_OUT = LOG_FLAGS_OUTGOING | LOG_FLAGS_BOUNDARY;
export const LOG_FLAGS_INCOMING_MESSAGE = LOG_FLAGS_INCOMING | LOG_FLAGS_MESSAGE;
export const LOG_FLAGS_OUTGOING_MESSAGE = LOG_FLAGS_OUTGOING | LOG_FLAGS_MESSAGE;

export class LogDictionary {
    constructor() {
        this._nextKey = 1;
        this._keyByString = {};
        this._stringByKey = {};
    }
    
    getKey(str) {
        if (this._keyByString[str]) {
            return this._keyByString[str];
        } else {
            const newKey = this._nextKey++;
            this.setString(newKey, str);
            return newKey;
        }
    }
    
    getString(key) {
        return this._stringByKey[newKey];
    }

    setString(key, str) {
        this._keyByString[str] = key;
        this._stringByKey[key] = str;
    }
}

export class LogCommandFactory {
    
    constructor() {
        this._dictionary = new LogDictionary();
    }

    get dictionary() {
        return this._dictionary;
    }

    getKey(str) {
        return this._dictionary.getKey(str);
    }

    getString(key) {
        return this._dictionary.getString(key);
    }

    createDictionaryEntry(numKey, strValue) {
        return {
            op: OPCODE_DICTIONARY_ENTRY,
            k: numKey,
            v: strValue
        };
    }

    createThreadStart(key, processId, processName, threadId, threadName, startTime) {
        return {
            op: OPCODE_THREAD_START,
            K: key,
            pid: processId,
            tid: threadId,
            pn: this.getKey(processName),
            tn: this.getKey(threadName),
            t0: startTime
        };
    }

    createMessageStart(threadKey, time, level, flags, componentId, messageId) {
        return {
            op: OPCODE_MESSAGE_START,
            K: threadKey,
            t: time,
            l: level,
            f: flags,
            c: this.getKey(componentId),
            m: this.getKey(messageId)
        };
    }

    createCloseActivityStart(threadKey, time, level) {
        return {
            op: OPCODE_CLOSE_ACTIVITY_START,
            K: threadKey,
            t: time,
            l: level
        };
    }

    createKvpKey(keyName) {
        return {
            op: OPCODE_KVP_KEY,
            k: this.getKey(keyName),
        };
    }

    createKvpValueNull(key) {
        return {
            op: OPCODE_KVP_VALUE_NULL
        };
    }

    createKvpValueString(value) {
        return {
            op: OPCODE_KVP_VALUE_STRING,
            v: this.getKey(value)
        };
    }

    createKvpValueNumber(value) {
        return {
            op: OPCODE_KVP_VALUE_NUMBER,
            v: value
        };
    }

    createKvpValueBoolean(value) {
        return {
            op: OPCODE_KVP_VALUE_BOOLEAN,
            v: value
        };
    }
    
    createException(type, message) {
        return {
            op: OPCODE_EXCEPTION,
            t: this.getKey(type),
            m: this.getKey(message)
        };
    }
 
    createMessageEnd() {
        return {
            op: OPCODE_MESSAGE_END
        };
    }

    createCloseActivityEnd() {
        return {
            op: OPCODE_CLOSE_ACTIVITY_END
        };
    }
    
    createChangeThreadName(threadKey, newName) {
        return {
            op: OPCODE_CHANGE_THREAD_NAME,
            K: threadKey,
            n: this.getKey(newName)
        };
    }
}
