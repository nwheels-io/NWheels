///<reference path="../lib/typings/jasmine/jasmine.d.ts" />

namespace UIDL.Tests
{
    describe("UIDLEvent", () => {
        it('CanInvokeSingleHandler', () => {
            //- arrange

            let log: string[] = [];
            let event = new UIDLEvent<string>();

            event.bind((arg: string) => {
                log.push(`test-handler(${arg})`);
            });

            //- act

            event.raise('ABC');

            //- assert

            expect(log).toEqual(['test-handler(ABC)']);
        });

        //-------------------------------------------------------------------------------------------------------------

        it('CanInvokeMultipleHandlers', () => {
            //- arrange

            let log: string[] = [];
            let event = new UIDLEvent<string>();

            event.bind((arg: string) => {
                log.push(`test-handler-1(${arg})`);
            });

            event.bind((arg: string) => {
                log.push(`test-handler-2(${arg})`);
            });

            //- act

            event.raise('ABC');

            //- assert

            expect(log).toEqual([
                'test-handler-1(ABC)',
                'test-handler-2(ABC)',
            ]);
        });

        //-------------------------------------------------------------------------------------------------------------

        it('CanUnbindHandler', () => {
            //- arrange

            let log: string[] = [];
            let event = new UIDLEvent<string>();

            var handler1 = ((arg: string) => {
                log.push(`test-handler-1(${arg})`);
            });
            var handler2 = ((arg: string) => {
                log.push(`test-handler-2(${arg})`);
            });
            var handler3 = ((arg: string) => {
                log.push(`test-handler-3(${arg})`);
            });

            event.bind(handler1);
            event.bind(handler2);
            event.bind(handler3);

            //- act

            event.unbind(handler2);
            event.raise('ABC');

            //- assert

            expect(log).toEqual([
                'test-handler-1(ABC)',
                'test-handler-3(ABC)',
            ]);
        });
    });
}