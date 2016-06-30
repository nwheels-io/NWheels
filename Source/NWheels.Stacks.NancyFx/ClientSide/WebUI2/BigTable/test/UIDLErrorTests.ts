///<reference path="../lib/typings/jasmine/jasmine.d.ts" />

namespace UIDL.Tests
{
    enum MyEnum {
        First,
        Second
    }

    describe('UIDLError', () => {

        it('can be thrown and caught', () => {
            try {
                throw new UIDLError('T1','M1');
            } catch (err) {
                expect(err).toEqual(jasmine.any(UIDLError));
            }
        });

        //-------------------------------------------------------------------------------------------------------------

        it('implements type, code, data, and name', () => {
            const data = { x: 123 };

            try {
                throw new UIDLError('T1', 'M1', data);
            } catch (err) {
                expect(err).toEqual(jasmine.any(UIDLError));

                const uidlError = (<UIDLError>err);

                expect(uidlError.type()).toEqual('T1');
                expect(uidlError.code()).toEqual('M1');
                expect(uidlError.data()).toBe(data);
                expect(uidlError.name).toEqual('T1.M1');
            }
        });

        //-------------------------------------------------------------------------------------------------------------

        it('can be checked with expect.toThrow', () => {
            expect(() => {
                throw new UIDLError('TEST', 'M1', 'test-message');
            }).toThrowError(UIDLError, 'test-message');
        });
    });
}