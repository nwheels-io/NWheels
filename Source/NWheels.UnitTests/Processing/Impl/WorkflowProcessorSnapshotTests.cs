using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using NWheels.Processing.Impl;
using ProtoBuf;
using ProtoBuf.Meta;

namespace NWheels.UnitTests.Processing.Impl
{
    [TestFixture]
    public class WorkflowProcessorSnapshotTests
    {
        [Test, Category("Manual")]
        public void CanSerializeWorkflowProcessorSnapshot()
        {
            //-- Arrange

            RuntimeTypeModel.Default.Add(typeof(WorkflowProcessorSnapshot), applyDefaultBehaviour: true);
            RuntimeTypeModel.Default.Add(typeof(TestWorkItem), true);

            var original = new WorkflowProcessorSnapshot() {
                //Actors = new WorkflowProcessorSnapshot.Actor[] {
                //    new WorkflowProcessorSnapshot.Actor() { Name = "A1", WorkItems = new object[] {
                //        new TestWorkItem { DayOfWeek = DayOfWeek.Tuesday }
                //    } },
                //    new WorkflowProcessorSnapshot.Actor() { Name = "A2", WorkItems = new object[] {
                //        new TestWorkItem { DayOfWeek = DayOfWeek.Monday }, 
                //        new TestWorkItem { DayOfWeek = DayOfWeek.Friday }
                //    } },
                //},
                AwaitList = new WorkflowProcessorSnapshot.ActorAwaitList() {
                    Entries = new WorkflowProcessorSnapshot.ActorAwaitListEntry[] {
                        //new WorkflowProcessorSnapshot.ActorAwaitListEntry() {
                        //    EventClrType = "Type1, Assembly1",
                        //    EventKey = "12345",
                        //    ActorNames = new[] { "A1", "A2", "A3" }
                        //},
                        //new WorkflowProcessorSnapshot.ActorAwaitListEntry() {
                        //    EventClrType = "Type2, Assembly2",
                        //    EventKey = new WorkflowProcessorSnapshot.Primitive<int>(12345),
                        //    ActorNames = new[] { "A4", "A5" }
                        //},
                        new WorkflowProcessorSnapshot.ActorAwaitListEntry() {
                            EventClrType = "Type3, Assembly3",
                            EventKey = new WorkflowProcessorSnapshot.Primitive<DayOfWeek>(DayOfWeek.Monday),
                            ActorNames = new[] { "A6" }
                        },
                        new WorkflowProcessorSnapshot.ActorAwaitListEntry() {
                            EventClrType = "Type4, Assembly4",
                            EventKey = new WorkflowProcessorSnapshot.Primitive<DayOfWeek>(DayOfWeek.Monday),
                            ActorNames = new[] { "A7" }
                        }
                    }
                }
            };


            //-- Act

            var stream = new MemoryStream();
            Serializer.Serialize<WorkflowProcessorSnapshot>(stream, original);
            
            Console.WriteLine(stream.Length);
            stream.Position = 0;

            using ( var file = File.Create(@"C:\Temp\snapshot.ptb") )
            {
                stream.CopyTo(file);
            }

            var deserialized = Serializer.Deserialize<WorkflowProcessorSnapshot>(stream);

            //-- Assert


        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class TestWorkItem
        {
            public DayOfWeek DayOfWeek { get; set; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class TestEventKey
        {
            public long Value { get; set; }
        }
    }
}
