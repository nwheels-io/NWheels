///<reference path="../../lib/typings/smartFormat/smartFormat.d.ts" />

namespace UIDL.Tests
{
    describe('lib/SmartFormat', () => {

        it('can format object using property placeholders', () => {
            //- arrange

            const template = 'Hello, {name}!';
            const data = { name: 'John' };

            //- act

            const result = Smart.format(template, data);

            //- assert

            expect(result).toEqual('Hello, John!');
        });

        //-------------------------------------------------------------------------------------------------------------

        it('can format array using indexed placeholders', () => {
            //- arrange

            const template = 'Hello, {0}! Are you {1}?';
            const data = ['John', 'Smith'];

            //- act

            const result = Smart.format(template, data);

            //- assert

            expect(result).toEqual('Hello, John! Are you Smith?');
        });

    });
}