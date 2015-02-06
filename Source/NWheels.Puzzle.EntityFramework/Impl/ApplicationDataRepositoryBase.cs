using System;
using System.Collections.Generic;
using System.Data.Entity.Core.Objects;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NWheels.Entities;
using NWheels.Puzzle.EntityFramework.Conventions;

namespace NWheels.Puzzle.EntityFramework.Impl
{
    public abstract class ApplicationDataRepositoryBase : IApplicationDataRepository
    {
        private readonly UnitOfWork _unitOfWork;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected ApplicationDataRepositoryBase(UnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void Dispose()
        {
            _unitOfWork.Dispose();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public abstract Type[] GetEntityTypesInRepository();

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void CommitChanges()
        {
            _unitOfWork.CommitChanges();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void RollbackChanges()
        {
            _unitOfWork.RollbackChanges();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public bool IsAutoCommitMode
        {
            get
            {
                return _unitOfWork.IsAutoCommitMode;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public UnitOfWorkState UnitOfWorkState
        {
            get
            {
                return _unitOfWork.UnitOfWorkState;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        internal void ValidateOperationalState()
        {
            _unitOfWork.ValidateOperationalState();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        internal ObjectSet<T> CreateObjectSet<T>() where T : class
        {
            return _unitOfWork.ObjectContext.CreateObjectSet<T>();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        internal UnitOfWork UnitOfWork
        {
            get
            {
                return _unitOfWork;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        internal ObjectContext ObjectContext
        {
            get
            {
                return _unitOfWork.ObjectContext;
            }
        }
    }
}
