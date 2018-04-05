import * as ThreadLog from './thread-log';
import * as Protocol from './thread-log-protocol';

describe('thread-log', () => {

    let _factory = null;

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    function makeThread(key, processId, processName, threadId, threadName, startTime) {
        const threadCommand = _factory.createThreadStart(key, processId, processName, threadId, threadName, startTime);
        const thread = new ThreadLog.Thread(threadCommand);
        return thread;
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    function makeMessage(time, componentId, messageId, level = Protocol.LOG_LEVEL_VERBOSE, flags = Protocol.LOG_FLAGS_NONE) {
        const command = _factory.createMessageStart(
            999,
            time,
            level,
            flags,
            componentId,
            messageId
        );

        const message = new ThreadLog.Message(command);
        return message;
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    function makeActivity(time, componentId, messageId, level = Protocol.LOG_LEVEL_VERBOSE, flags = Protocol.LOG_FLAGS_NONE) {
        return makeMessage(
            time,
            componentId,
            messageId,
            level,
            flags | Protocol.LOG_FLAGS_ACTIVITY
        );
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    beforeEach(() => {
        _factory = new Protocol.LogCommandFactory();
    });

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    describe('Message', () => {

        test('constructor', () => {
            const command = _factory.createMessageStart(
                123,
                987,
                Protocol.LOG_LEVEL_INFO,
                Protocol.LOG_FLAGS_INCOMING_MESSAGE,
                'C1',
                'M1');

            const message = new ThreadLog.Message(command);

            expect(message.time).toBe(987);
            expect(message.level).toBe(Protocol.LOG_LEVEL_INFO);
            expect(message.flags).toBe(Protocol.LOG_FLAGS_INCOMING_MESSAGE);
            expect(message.componentId).toBe(_factory.getKey('C1'));
            expect(message.messageId).toBe(_factory.getKey('M1'));

            expect(message.parent).toBeNull();
            expect(message.firstChild).toBeNull();
            expect(message.lastChild).toBeNull();
            expect(message.prevSibling).toBeNull();
            expect(message.nextSibling).toBeNull();
        });

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        test('appendChild first', () => {
            const parent = makeMessage(987, 'C1', 'M1', Protocol.LOG_LEVEL_INFO, Protocol.LOG_FLAGS_INCOMING_MESSAGE);
            const child1 = makeMessage(988, 'C2', 'M2', Protocol.LOG_LEVEL_VERBOSE, Protocol.LOG_FLAGS_NONE);

            parent.appendChild(child1);

            expect(parent.parent).toBeNull();
            expect(parent.firstChild).toBe(child1);
            expect(parent.lastChild).toBe(child1);
            expect(parent.prevSibling).toBeNull();
            expect(parent.nextSibling).toBeNull();

            expect(child1.parent).toBe(parent);
            expect(child1.firstChild).toBeNull();
            expect(child1.lastChild).toBeNull();
            expect(child1.prevSibling).toBeNull();
            expect(child1.nextSibling).toBeNull();
        });

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        test('appendChild second', () => {
            const parent = makeMessage(987, 'C1', 'M1', Protocol.LOG_LEVEL_INFO, Protocol.LOG_FLAGS_INCOMING_MESSAGE);
            const child1 = makeMessage(988, 'C2', 'M2', Protocol.LOG_LEVEL_VERBOSE, Protocol.LOG_FLAGS_NONE);
            const child2 = makeMessage(989, 'C3', 'M3', Protocol.LOG_LEVEL_VERBOSE, Protocol.LOG_FLAGS_NONE);

            parent.appendChild(child1);
            parent.appendChild(child2);

            expect(parent.parent).toBeNull();
            expect(parent.firstChild).toBe(child1);
            expect(parent.lastChild).toBe(child2);
            expect(parent.prevSibling).toBeNull();
            expect(parent.nextSibling).toBeNull();

            expect(child1.parent).toBe(parent);
            expect(child1.firstChild).toBeNull();
            expect(child1.lastChild).toBeNull();
            expect(child1.prevSibling).toBeNull();
            expect(child1.nextSibling).toBe(child2);

            expect(child2.parent).toBe(parent);
            expect(child2.firstChild).toBeNull();
            expect(child2.lastChild).toBeNull();
            expect(child2.prevSibling).toBe(child1);
            expect(child2.nextSibling).toBeNull();
        });

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        test('addValue and getValues', () => {
            const message = makeMessage(987, 'C1', 'M1', Protocol.LOG_LEVEL_INFO, Protocol.LOG_FLAGS_NONE);
            
            message.addValue(111, 222, 'string');
            message.addValue(333, 444, 'number');
            message.addValue(555, true, 'boolean');

            const values = message.values;

            expect(values).toMatchObject({ 
                '111': { value: 222, type: 'string' },
                '333': { value: 444, type: 'number' },
                '555': { value: true, type: 'boolean' }
            });
        });

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        test('getValue', () => {
            const message = makeMessage(987, 'C1', 'M1', Protocol.LOG_LEVEL_INFO, Protocol.LOG_FLAGS_NONE);
            
            message.addValue(111, 222, 'string');
            message.addValue(333, 444, 'number');
            message.addValue(555, true, 'boolean');

            expect(message.getValue(111)).toMatchObject({ value: 222, type: 'string' });
            expect(message.getValue(999)).toBeUndefined();
        });

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        test('close activity', () => {
            const message = makeMessage(1500, 'C1', 'A1', Protocol.LOG_LEVEL_INFO, Protocol.LOG_FLAGS_ACTIVITY);

            message.setEndTime(1750);

            expect(message.duration).toBe(250);
            expect(message.exception).toBeNull();
        });

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        test('close activity with exception', () => {
            const message = makeMessage(1500, 'C1', 'A1', Protocol.LOG_LEVEL_INFO, Protocol.LOG_FLAGS_ACTIVITY);

            message.addValue(111, 222, 'string');
            message.setEndTime(2000);
            message.setException(333, 444);

            expect(message.getValue(111)).toEqual({ value: 222, type: 'string' });
            expect(message.exception).toEqual({ type: 333, message: 444 });
        });
        
    });

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    describe('Thread', () => {

        test('initial state', () => {
            const startTime = new Date(2010, 1, 2, 3, 4, 5, 6, 7).getTime();
            const thread = makeThread(123, 234, 'process1', 345, 'thread1', startTime);

            expect(thread.key).toBe(123);
            expect(thread.processId).toBe(234);
            expect(thread.processName).toBe(_factory.getKey('process1'));
            expect(thread.threadId).toBe(345);
            expect(thread.threadName).toBe(_factory.getKey('thread1'));
            expect(thread.startedAt).toBe(startTime);

            expect(thread.rootActivity).toBeNull();
            expect(thread.currentActivity).toBeNull();

            expect(thread.isClosed).toBe(false);
        });

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        test('add root activity', () => {
            const thread = makeThread(123, 234, 'process1', 345, 'thread1', new Date().getTime());
            const activity = makeActivity(
                1,
                Protocol.LOG_LEVEL_INFO,
                Protocol.LOG_FLAGS_ACTIVITY,
                'C1',
                'M1');

            thread.append(activity);

            expect(thread.isClosed).toBe(false);
            expect(thread.rootActivity).toBe(activity);
            expect(thread.currentActivity).toBe(activity);
            expect(thread.rootActivity.parent).toBeNull();
            expect(thread.rootActivity.firstChild).toBeNull();
        });

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        test('add multiple activities and messages', () => {
            const thread = makeThread(123, 234, 'process1', 345, 'thread1', new Date().getTime());
            const activity1 = makeActivity(1, 'C1', 'M1', Protocol.LOG_LEVEL_INFO, Protocol.LOG_FLAGS_ACTIVITY);
            const message2 = makeMessage(2, 'C2', 'M2', Protocol.LOG_LEVEL_DEBUG, Protocol.LOG_FLAGS_NONE);
            const activity3 = makeActivity(3, 'C3', 'M3', Protocol.LOG_LEVEL_INFO, Protocol.LOG_FLAGS_ACTIVITY);
            const message4 = makeMessage(4, 'C4', 'M4', Protocol.LOG_LEVEL_DEBUG, Protocol.LOG_FLAGS_NONE);

            thread.append(activity1);
            thread.append(message2);
            thread.append(activity3);
            thread.append(message4);

            expect(thread.isClosed).toBe(false);
            expect(thread.rootActivity).toBe(activity1);
            expect(thread.currentActivity).toBe(activity3);
            expect(activity1.parent).toBeNull();
            expect(message2.parent).toBe(activity1);
            expect(activity3.parent).toBe(activity1);
            expect(message4.parent).toBe(activity3);
        });

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        test('close activity', () => {
            const thread = makeThread(123, 234, 'process1', 345, 'thread1', new Date().getTime());
            const activity1 = makeActivity(1, 'C1', 'M1', Protocol.LOG_LEVEL_INFO, Protocol.LOG_FLAGS_ACTIVITY);
            const message2 = makeMessage(2, 'C2', 'M2', Protocol.LOG_LEVEL_DEBUG, Protocol.LOG_FLAGS_NONE);
            const activity3 = makeActivity(3, 'C3', 'M3', Protocol.LOG_LEVEL_INFO, Protocol.LOG_FLAGS_ACTIVITY);
            const message4 = makeMessage(4, 'C4', 'M4', Protocol.LOG_LEVEL_DEBUG, Protocol.LOG_FLAGS_NONE);
            const message5 = makeMessage(5, 'C5', 'M5', Protocol.LOG_LEVEL_VERBOSE, Protocol.LOG_FLAGS_NONE);

            thread.append(activity1);
            thread.append(message2);
            thread.append(activity3);
            thread.append(message4);
            thread.closeActivity();
            thread.append(message5);

            expect(thread.isClosed).toBe(false);
            expect(thread.rootActivity).toBe(activity1);
            expect(thread.currentActivity).toBe(activity1);
            expect(activity1.parent).toBeNull();
            expect(message2.parent).toBe(activity1);
            expect(activity3.parent).toBe(activity1);
            expect(message4.parent).toBe(activity3);
            expect(message5.parent).toBe(activity1);
        });
    });
});
