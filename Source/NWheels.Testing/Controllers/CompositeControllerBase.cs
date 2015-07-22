using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NWheels.Logging.Core;
using NWheels.Processing.Workflows;
using NWheels.Processing.Workflows.Core;

namespace NWheels.Testing.Controllers
{
    public abstract class CompositeControllerBase<TSubController> : ControllerBase
        where TSubController : ControllerBase
    {
        private readonly List<TSubController> _subControllers;
        private RevertableSequence _loadSequence;
        private RevertableSequence _activateSequence;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected CompositeControllerBase(ControllerBase parentController)
            : base(parentController)
        {
            _subControllers = new List<TSubController>();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected CompositeControllerBase(IPlainLog log)
            : base(log)
        {
            _subControllers = new List<TSubController>();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override ControllerBase[] GetSubControllers()
        {
            return _subControllers.Cast<ControllerBase>().ToArray();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public IReadOnlyList<TSubController> SubControllers
        {
            get
            {
                return _subControllers;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected void AddSubController(TSubController subController)
        {
            _subControllers.Add(subController);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override void OnLoad()
        {
            _loadSequence = new RevertableSequence(new LoadSequenceCodeBehind(this));
            _loadSequence.Perform();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override void OnActivate()
        {
            _activateSequence = new RevertableSequence(new ActivateSequenceCodeBehind(this));
            _activateSequence.Perform();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override void OnDeactivate()
        {
            _activateSequence.Revert();
            _activateSequence = null;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override void OnUnload()
        {
            _loadSequence.Revert();
            _loadSequence = null;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private class LoadSequenceCodeBehind : IRevertableSequenceCodeBehind
        {
            private readonly CompositeControllerBase<TSubController> _ownerController;

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public LoadSequenceCodeBehind(CompositeControllerBase<TSubController> ownerController)
            {
                _ownerController = ownerController;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public void BuildSequence(IRevertableSequenceBuilder sequence)
            {
                sequence.ForEach(() => _ownerController._subControllers)
                    .OnPerform(
                        (subController, index, isLast) => {
                            subController.Load();
                        })
                    .OnRevert(
                        (subController, index, isLast) => {
                            subController.Unload();
                        });
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private class ActivateSequenceCodeBehind : IRevertableSequenceCodeBehind
        {
            private readonly CompositeControllerBase<TSubController> _ownerController;

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public ActivateSequenceCodeBehind(CompositeControllerBase<TSubController> ownerController)
            {
                _ownerController = ownerController;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public void BuildSequence(IRevertableSequenceBuilder sequence)
            {
                sequence.ForEach(() => _ownerController._subControllers)
                    .OnPerform(
                        (subController, index, isLast) => {
                            subController.Activate();
                        })
                    .OnRevert(
                        (subController, index, isLast) => {
                            subController.Deactivate();
                        });
            }
        }
    }
}

