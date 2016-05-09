using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using NWheels.Hosting;
using Shouldly;

namespace NWheels.UnitTests.Hosting
{
    [TestFixture]
    public class EnvironmentConfigurationTests
    {
        [TestCase("M1", @"C:\ZZZ", "E1", EnvironmentConfiguration.LookupResult.MatchedByMachine)]
        [TestCase("M2", @"C:\ZZZ", "E2", EnvironmentConfiguration.LookupResult.MatchedByMachine)]
        [TestCase("M3", @"C:\ZZZ", "E3", EnvironmentConfiguration.LookupResult.MatchedDefaultFallback)]
        public void CanGetEnvironmentDefinedAsSingleMachine(
            string inputMachine, 
            string inputFolder, 
            string expectedEnvironmentName, 
            EnvironmentConfiguration.LookupResult expectedResult)
        {
            //-- arrange

            var config = new EnvironmentConfiguration() {
                Environments = new List<EnvironmentConfiguration.Environment>() {
                    new EnvironmentConfiguration.Environment() {
                        Name = "E1",
                        MachineName = "M1",
                    },
                    new EnvironmentConfiguration.Environment() {
                        Name = "E2",
                        MachineName = "M2",
                    },
                    new EnvironmentConfiguration.Environment() {
                        Name = "E3",
                        MachineName = EnvironmentConfiguration.DefaultMachineName
                    }
                }
            };

            //-- act

            EnvironmentConfiguration.Environment actualEnvironment;
            var actualResult = config.TryGetEnvironment(inputMachine, inputFolder, out actualEnvironment);

            //-- assert

            if (expectedEnvironmentName != null)
            {
                actualEnvironment.ShouldNotBeNull();
                actualEnvironment.Name.ShouldBe(expectedEnvironmentName);
            }
            
            actualResult.ShouldBe(expectedResult);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [TestCase("M1", @"c:\f1\", "E1", EnvironmentConfiguration.LookupResult.MatchedByMachineAndFolder)]
        [TestCase("M1", @"C:\f2", "E2", EnvironmentConfiguration.LookupResult.MatchedByMachineAndFolder)]
        [TestCase("M1", @"C:\F3", "E3", EnvironmentConfiguration.LookupResult.MatchedDefaultFallback)]
        public void CanGetEnvironmentDefinedAsSingleMachineAndFolder(
            string inputMachine,
            string inputFolder,
            string expectedEnvironmentName,
            EnvironmentConfiguration.LookupResult expectedResult)
        {
            //-- arrange

            var config = new EnvironmentConfiguration()
            {
                Environments = new List<EnvironmentConfiguration.Environment>() {
                    new EnvironmentConfiguration.Environment() {
                        Name = "E1",
                        MachineName = "M1",
                        DeploymentFolder = @"C:\F1"
                    },
                    new EnvironmentConfiguration.Environment() {
                        Name = "E2",
                        MachineName = "M1",
                        DeploymentFolder = @"C:\F2\"
                    },
                    new EnvironmentConfiguration.Environment() {
                        Name = "E3",
                        MachineName = EnvironmentConfiguration.DefaultMachineName
                    }
                }
            };

            //-- act

            EnvironmentConfiguration.Environment actualEnvironment;
            var actualResult = config.TryGetEnvironment(inputMachine, inputFolder, out actualEnvironment);

            //-- assert

            if (expectedEnvironmentName != null)
            {
                actualEnvironment.ShouldNotBeNull();
                actualEnvironment.Name.ShouldBe(expectedEnvironmentName);
            }

            actualResult.ShouldBe(expectedResult);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [TestCase("M1", @"c:\f1\", "E1", EnvironmentConfiguration.LookupResult.MatchedByMachine)]
        [TestCase("M4", @"c:\f1\", "E2", EnvironmentConfiguration.LookupResult.MatchedByMachine)]
        [TestCase("M7", @"c:\f1\", null, EnvironmentConfiguration.LookupResult.NotFound)]
        public void CanGetEnvironmentDefinedAsMultipleMachines(
            string inputMachine,
            string inputFolder,
            string expectedEnvironmentName,
            EnvironmentConfiguration.LookupResult expectedResult)
        {
            //-- arrange

            var config = new EnvironmentConfiguration()
            {
                Environments = new List<EnvironmentConfiguration.Environment>() {
                    new EnvironmentConfiguration.Environment() {
                        Name = "E1",
                        Machines = new List<EnvironmentConfiguration.Machine>() {
                            new EnvironmentConfiguration.Machine() {
                                MachineName = "M1"
                            },
                            new EnvironmentConfiguration.Machine() {
                                MachineName = "M2"
                            },
                        }
                    },
                    new EnvironmentConfiguration.Environment() {
                        Name = "E2",
                        Machines = new List<EnvironmentConfiguration.Machine>() {
                            new EnvironmentConfiguration.Machine() {
                                MachineName = "M3"
                            },
                            new EnvironmentConfiguration.Machine() {
                                MachineName = "M4"
                            },
                        }
                    },
                }
            };

            //-- act

            EnvironmentConfiguration.Environment actualEnvironment;
            var actualResult = config.TryGetEnvironment(inputMachine, inputFolder, out actualEnvironment);

            //-- assert

            if (expectedEnvironmentName != null)
            {
                actualEnvironment.ShouldNotBeNull();
                actualEnvironment.Name.ShouldBe(expectedEnvironmentName);
            }

            actualResult.ShouldBe(expectedResult);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [TestCase("M1", @"c:\f1\", "E1", EnvironmentConfiguration.LookupResult.MatchedByMachineAndFolder)]
        [TestCase("M1", @"c:\f4\", "E2", EnvironmentConfiguration.LookupResult.MatchedByMachineAndFolder)]
        [TestCase("M2", @"c:\fZ\", null, EnvironmentConfiguration.LookupResult.NotFound)]
        public void CanGetEnvironmentDefinedAsMultipleMachinesWithCommonFolders(
            string inputMachine,
            string inputFolder,
            string expectedEnvironmentName,
            EnvironmentConfiguration.LookupResult expectedResult)
        {
            //-- arrange

            var config = new EnvironmentConfiguration()
            {
                Environments = new List<EnvironmentConfiguration.Environment>() {
                    new EnvironmentConfiguration.Environment() {
                        Name = "E1",
                        DeploymentFolders = new List<EnvironmentConfiguration.DeploymentFolder>() {
                            new EnvironmentConfiguration.DeploymentFolder() {
                                Path = @"c:\f1"
                            },
                            new EnvironmentConfiguration.DeploymentFolder() {
                                Path = @"c:\f2"
                            }    
                        },
                        Machines = new List<EnvironmentConfiguration.Machine>() {
                            new EnvironmentConfiguration.Machine() {
                                MachineName = "M1"
                            },
                            new EnvironmentConfiguration.Machine() {
                                MachineName = "M2"
                            },
                        }
                    },
                    new EnvironmentConfiguration.Environment() {
                        Name = "E2",
                        DeploymentFolders = new List<EnvironmentConfiguration.DeploymentFolder>() {
                            new EnvironmentConfiguration.DeploymentFolder() {
                                Path = @"c:\f3"
                            },
                            new EnvironmentConfiguration.DeploymentFolder() {
                                Path = @"c:\f4"
                            }    
                        },
                        Machines = new List<EnvironmentConfiguration.Machine>() {
                            new EnvironmentConfiguration.Machine() {
                                MachineName = "M1"
                            },
                            new EnvironmentConfiguration.Machine() {
                                MachineName = "M2"
                            },
                        }
                    },
                }
            };

            //-- act

            EnvironmentConfiguration.Environment actualEnvironment;
            var actualResult = config.TryGetEnvironment(inputMachine, inputFolder, out actualEnvironment);

            //-- assert

            if (expectedEnvironmentName != null)
            {
                actualEnvironment.ShouldNotBeNull();
                actualEnvironment.Name.ShouldBe(expectedEnvironmentName);
            }

            actualResult.ShouldBe(expectedResult);
        }
    }
}
