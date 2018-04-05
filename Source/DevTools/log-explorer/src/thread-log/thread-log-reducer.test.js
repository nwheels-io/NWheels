import * as ThreadLog from './thread-log';
import * as Protocol from './thread-log-protocol';
import { ThreadLogReducer } from './thread-log-reducer';

describe('thread-log-reducer', () => {

    let _factory = null;

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    beforeEach(() => {
        _factory = new Protocol.LogCommandFactory();
    });

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    test('single message thread', () => {

        //-- arrange

        const commands = [
            _factory.createThreadStart(111, 222, 'pid-222', 333, 'tid-333', 1000),
            _factory.createMessageStart(111, 300, Protocol.LOG_LEVEL_INFO, Protocol.LOG_FLAGS_ACTIVITY, 'C1', 'A1'),
            _factory.createKvpKey('num'),
            _factory.createKvpValueNumber(123),
            _factory.createMessageEnd(),
            _factory.createCloseActivityStart(111, 700, Protocol.LOG_LEVEL_INFO),
            _factory.createCloseActivityEnd(),
        ];

        let completedThreads = [];
        const reducer = new ThreadLogReducer(thread => {
            completedThreads.push(thread);
        });

        //-- act

        commands.forEach(command => {
            reducer.push(command);
        });

        //-- assert

        expect(completedThreads.length).toBe(1);
        expect(completedThreads[0]).toBeInstanceOf(ThreadLog.Thread);
        expect(completedThreads[0].isClosed).toBe(true);

        const rootActivity = completedThreads[0].rootActivity;
        expect(rootActivity).toBeInstanceOf(ThreadLog.Message);
        expect(rootActivity.getValue(_factory.getKey('num'))).toEqual({ value: 123, type: 'number' });
        expect(rootActivity.duration).toBe(700 - 300);
        expect(rootActivity.firstChild).toBeNull();
        expect(rootActivity.nextSibling).toBeNull();
    });

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    test('nested activities and messages', () => {

        //-- arrange

        let completedThreads = [];
        const reducer = new ThreadLogReducer(thread => {
            completedThreads.push(thread);
        });

        //-- act

        reducer.push(_factory.createThreadStart(111, 222, 'pid-222', 333, 'tid-333', 1000)); 
        // T[111]->
        reducer.push(_factory.createMessageStart(111, 100, Protocol.LOG_LEVEL_INFO, Protocol.LOG_FLAGS_ACTIVITY, 'C1', 'A1'));
        reducer.push(_factory.createMessageEnd());
        // T[111]->C1::A1
        reducer.push(_factory.createMessageStart(111, 101, Protocol.LOG_LEVEL_INFO, Protocol.LOG_FLAGS_NONE, 'C2', 'M2'));
        reducer.push(_factory.createMessageEnd());
        // T[111]->C1::A1{ C2::M2
        reducer.push(_factory.createMessageStart(111, 102, Protocol.LOG_LEVEL_INFO, Protocol.LOG_FLAGS_ACTIVITY, 'C2', 'A3'));
        reducer.push(_factory.createMessageEnd());
        // T[111]->C1::A1{ C2::M2 ; C2::A3{
        reducer.push(_factory.createMessageStart(111, 103, Protocol.LOG_LEVEL_INFO, Protocol.LOG_FLAGS_NONE, 'C3', 'M4'));
        reducer.push(_factory.createMessageEnd());
        // T[111]->C1::A1{ C2::M2 ; C2::A3{ C3::M4
        reducer.push(_factory.createCloseActivityStart(111, 104, Protocol.LOG_LEVEL_INFO));
        reducer.push(_factory.createCloseActivityEnd());
        // T[111]->C1::A1{ C2::M2 ; C2::A3{ C3::M4 }
        reducer.push(_factory.createMessageStart(111, 105, Protocol.LOG_LEVEL_INFO, Protocol.LOG_FLAGS_NONE, 'C2', 'M5'));
        reducer.push(_factory.createMessageEnd());
        // T[111]->C1::A1{ C2::M2 ; C2::A3{ C3::M4 } ; C2::M5
        reducer.push(_factory.createCloseActivityStart(111, 106, Protocol.LOG_LEVEL_INFO));
        reducer.push(_factory.createCloseActivityEnd());
        // T[111]->C1::A1{ C2::M2 ; C2::A3{ C3::M4 } ; C2::M5 }

        //-- assert

        expect(completedThreads.length).toBe(1);
        expect(completedThreads[0]).toBeInstanceOf(ThreadLog.Thread);
        expect(completedThreads[0].isClosed).toBe(true);

        const activityA1 = completedThreads[0].rootActivity;
        expect(activityA1).toBeInstanceOf(ThreadLog.Message);
        expect(activityA1.parent).toBeNull();
        expect(activityA1.messageId).toBe(_factory.getKey('A1'));
        expect(activityA1.isActivity).toBe(true);
        expect(activityA1.duration).toBe(106 - 100);
        expect(activityA1.parent).toBeNull();
        expect(activityA1.nextSibling).toBeNull();

        const messageM2 = activityA1.firstChild;
        expect(messageM2).toBeInstanceOf(ThreadLog.Message);
        expect(messageM2.parent).toBe(activityA1);
        expect(messageM2.messageId).toBe(_factory.getKey('M2'));
        expect(messageM2.isActivity).toBe(false);
        expect(messageM2.duration).toBeUndefined();
        expect(messageM2.firstChild).toBeNull();

        const activityA3 = messageM2.nextSibling;
        expect(activityA3).toBeInstanceOf(ThreadLog.Message);
        expect(activityA3.parent).toBe(activityA1);
        expect(activityA3.messageId).toBe(_factory.getKey('A3'));
        expect(activityA3.isActivity).toBe(true);
        expect(activityA3.duration).toBe(104 - 102);

        const messageM4 = activityA3.firstChild;
        expect(messageM4).toBeInstanceOf(ThreadLog.Message);
        expect(messageM4.parent).toBe(activityA3);
        expect(messageM4.messageId).toBe(_factory.getKey('M4'));
        expect(messageM4.isActivity).toBe(false);
        expect(messageM4.duration).toBeUndefined();
        expect(messageM4.firstChild).toBeNull();
        expect(messageM4.nextSibling).toBeNull();
        
        const messageM5 = activityA3.nextSibling;
        expect(messageM5).toBeInstanceOf(ThreadLog.Message);
        expect(messageM5.parent).toBe(activityA1);
        expect(messageM5.messageId).toBe(_factory.getKey('M5'));
        expect(messageM5.isActivity).toBe(false);
        expect(messageM5.duration).toBeUndefined();
        expect(messageM5.firstChild).toBeNull();
        expect(messageM5.nextSibling).toBeNull();
    });

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    test('all kinds of values', () => {

        //-- arrange

        let completedThreads = [];
        const reducer = new ThreadLogReducer(thread => {
            completedThreads.push(thread);
        });

        //-- act

        reducer.push(_factory.createThreadStart(111, 222, 'pid-222', 333, 'tid-333', 1000)); 

        reducer.push(_factory.createMessageStart(111, 100, Protocol.LOG_LEVEL_INFO, Protocol.LOG_FLAGS_ACTIVITY, 'C1', 'A1'));
        reducer.push(_factory.createKvpKey('num'));
        reducer.push(_factory.createKvpValueNumber(123));
        reducer.push(_factory.createKvpKey('str'));
        reducer.push(_factory.createKvpValueString('abc'));
        reducer.push(_factory.createKvpKey('boo'));
        reducer.push(_factory.createKvpValueBoolean(true));
        reducer.push(_factory.createKvpKey('nothing'));
        reducer.push(_factory.createKvpValueNull());
        reducer.push(_factory.createMessageEnd());

        reducer.push(_factory.createCloseActivityStart(111, 106, Protocol.LOG_LEVEL_INFO));
        reducer.push(_factory.createKvpKey('endNum'));
        reducer.push(_factory.createKvpValueNumber(789));
        reducer.push(_factory.createKvpKey('endStr'));
        reducer.push(_factory.createKvpValueString('xyz'));
        reducer.push(_factory.createException('DemoException', 'demo error'));
        reducer.push(_factory.createCloseActivityEnd());

        //-- assert

        expect(completedThreads.length).toBe(1);
        expect(completedThreads[0]).toBeInstanceOf(ThreadLog.Thread);
        expect(completedThreads[0].isClosed).toBe(true);

        const activity = completedThreads[0].rootActivity;
        expect(activity).toBeInstanceOf(ThreadLog.Message);

        expect(activity.getValue(_factory.getKey('num'))).toEqual({ value: 123, type: 'number' });
        expect(activity.getValue(_factory.getKey('str'))).toEqual({ value: _factory.getKey('abc'), type: 'string' });
        expect(activity.getValue(_factory.getKey('boo'))).toEqual({ value: true, type: 'boolean' });
        expect(activity.getValue(_factory.getKey('nothing'))).toEqual({ value: null, type: null });
        expect(activity.getValue(_factory.getKey('endNum'))).toEqual({ value: 789, type: 'number' });
        expect(activity.getValue(_factory.getKey('endStr'))).toEqual({ value: _factory.getKey('xyz'), type: 'string' });
        expect(activity.exception).toEqual({ 
            type: _factory.getKey('DemoException'),
            message: _factory.getKey('demo error')
        });
    });
});
