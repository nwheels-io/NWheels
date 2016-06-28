namespace UIDL
{
    export class UIDLError {
        private _code: string;
        private _subCode: string;
        private _message: string;
        private _params: string[];

        //-------------------------------------------------------------------------------------------------------------

        public constructor(message: string, code?: string, subCode?: string, ...params: string[]) {
            this._message = message;
            this._code = code || ErrorCodes.General.CODE_UNSPECIFIED();
            this._subCode = subCode || null;
            this._params = params || null;
        };

        //-------------------------------------------------------------------------------------------------------------

        public static errorPropertyIsReadOnly(propertyName: string): UIDLError {
            return new UIDLError(
                `Property is read only: ${propertyName}`,
                ErrorCodes.General.CODE_PROPERTY_READ_ONLY(),
                null,
                propertyName);
        }
    }

    //-----------------------------------------------------------------------------------------------------------------

    namespace ErrorCodes
    {
        export class General {
            public static CODE_UNSPECIFIED(): string {
                return 'UNSPECIFIED';
            }
            public static CODE_PROPERTY_READ_ONLY(): string {
                return 'UNSPECIFIED';
            }
        }
    }
}
