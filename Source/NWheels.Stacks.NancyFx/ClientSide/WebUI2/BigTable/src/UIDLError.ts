namespace UIDL
{
    export class UIDLError extends Error {
        private _type: string;
        private _code: string;
        private _data: any;
        private _stack: any;

        //-------------------------------------------------------------------------------------------------------------

        public constructor(type: string, code: string, data?: any) {
            super(UIDLError.formatMessage(data));

            this.message = UIDLError.formatMessage(data);
            this.name = `${type}.${code}`;
            this._type = type;
            this._code = code;
            this._data = data || null;
            this._stack = (<any>new Error()).stack;
        }

        //-------------------------------------------------------------------------------------------------------------

        public type(): string {
            return this._type;
        }

        //-------------------------------------------------------------------------------------------------------------

        public code(): string {
            return this._code;
        }

        //-------------------------------------------------------------------------------------------------------------

        public data(): any {
            return this._data;
        }

        //-------------------------------------------------------------------------------------------------------------

        public dataAs<T>(): T {
            return (this._data as T);
        }

        //-------------------------------------------------------------------------------------------------------------

        public toString(): string {
            return `${this._type}.${this._code}: ${this.message}`;
        }

        //-------------------------------------------------------------------------------------------------------------

        public static factory(): UIDLErrorFactory {
            return new UIDLErrorFactory();
        }

        //-------------------------------------------------------------------------------------------------------------

        private static formatMessage(data: any): string {
            if (!data) {
                return 'Error';
            }

            if (typeof data === 'string') {
                return data;
            }

            let text = '';

            if (data.message) {
                text += data.message.toString();
            }

            let firstProperty = true;

            for (var p in data) {
                if (p !== 'message' && data.hasOwnProperty(p) && data[p] != null) {
                    text += (firstProperty ? ': ' : ', ') + p + '=' + data[p].toString();
                    firstProperty = false;
                }
            }

            return text;
        }
    }

    //-----------------------------------------------------------------------------------------------------------------

    export class UIDLErrorList {
        static TYPE_GENERAL(): string { return 'GENERAL'; }
        static CODE_GENERAL_UNSPECIFIED(): string { return 'UNSPECIFIED'; }
        static CODE_GENERAL_NOT_SUPPORTED(): string { return 'NOT_SUPPORTED'; }
    }

    //-----------------------------------------------------------------------------------------------------------------

    export class UIDLErrorFactory {
        public generalUnspecified(data?: any): UIDLError {
            return new UIDLError(UIDLErrorList.TYPE_GENERAL(), UIDLErrorList.CODE_GENERAL_UNSPECIFIED());
        }
        public generalNotSupported(component: string, feature: string): UIDLError {
            return new UIDLError(UIDLErrorList.TYPE_GENERAL(), UIDLErrorList.CODE_GENERAL_NOT_SUPPORTED(), {
                component: component,
                feature: feature
            });
        }
    }
}
