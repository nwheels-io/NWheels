using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NWheels.UI.Uidl;
using NWheels.Utilities;

namespace NWheels.UI
{
    public class UIOperationContext : IDisposable
    {
        public UIOperationContext(
            ApplicationEntityService entityService,
            ApiCallType callType,
            ApiCallResultType resultType,
            string target,
            string contract,
            string operation,
            string format = null,
            string entity = null,
            ApplicationEntityService.QueryOptions query = null)
        {
            if (_s_current != null)
            {
                throw new InvalidOperationException("Another UIOperationContext instance is already attached to the current thread.");
            }

            _s_current = this;

            this.EntityService = entityService;

            this.ApiCallType = callType;
            this.ApiResultType = resultType;
            this.ApiTargetType = ParseUtility.Parse<ApiCallTargetType>(target);

            this.ContractName = contract;
            this.OperationName = operation;
            this.OutputFormatIdName = format;
            this.EntityName = entity;

            this.Query = query;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        #region Implementation of IDisposable

        public void Dispose()
        {
            _s_current = null;
        }

        #endregion

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public ApplicationEntityService EntityService { get; private set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public ApiCallType ApiCallType { get; private set; }
        public ApiCallTargetType ApiTargetType { get; private set; }
        public ApiCallResultType ApiResultType { get; private set; }
        public ApplicationEntityService.QueryOptions Query { get; private set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public string ContractName { get; private set; }
        public string OperationName { get; private set; }
        public string OutputFormatIdName { get; private set; }
        public string EntityName { get; private set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [ThreadStatic]
        private static UIOperationContext _s_current;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static UIOperationContext Current
        {
            get
            {
                return _s_current;
            }
        }
    }
}
