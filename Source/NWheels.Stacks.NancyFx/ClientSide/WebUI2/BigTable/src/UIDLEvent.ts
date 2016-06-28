namespace UIDL
{
    export class UIDLEvent<T> {
        private _handlers: ((args: T) => void)[] = [ ];

        //-------------------------------------------------------------------------------------------------------------

        bind(handler: (args: T) => void): void {
            this._handlers.push(handler);
        }

        //-------------------------------------------------------------------------------------------------------------

        unbind(handler: (args: T) => void): void {
            this._handlers = this._handlers.filter(h => h !== handler);
        }

        //-------------------------------------------------------------------------------------------------------------

        raise(args: T) {
            this._handlers.slice(0).forEach(h => h(args));
        }
    }
}
