using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FakeItEasy;
using IdentityServer4.EntityFramework.Entities;
using IdentityServer4.EntityFramework.Interfaces;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using safnet.Identity.Api.Infrastructure.Persistence;
using safnet.TestHelper.AsyncDbSet;
using Shouldly;
using Secret = IdentityServer4.Models.Secret;

// ReSharper disable InconsistentNaming

namespace safnet.Identity.Api.UnitTests.Infrastructure.Persistence
{
    [TestFixture]
    public class ClientRepositoryTests
    {
        [SetUp]
        protected virtual void SetUp()
        {
            ConfigurationDbContext = A.Fake<IConfigurationDbContext>();

            System = new ClientRepository(ConfigurationDbContext);
        }

        protected ClientRepository System;
        protected IConfigurationDbContext ConfigurationDbContext;

        [TestFixture]
        public class Modification : ClientRepositoryTests
        {
            [SetUp]
            protected override void SetUp()
            {
                base.SetUp();

                MockDbSet = A.Fake<DbSet<Client>>();
                A.CallTo(() => ConfigurationDbContext.Clients).Returns(MockDbSet);
            }

            protected DbSet<Client> MockDbSet;

            [TestFixture]
            public class When_creating_new_Client : Modification
            {
                [TestFixture]
                public class Given_null_model : When_creating_new_Client
                {
                    [Test]
                    public void Then_throw_ArgumentNullException()
                    {
                        Func<Task<IdentityServer4.Models.Client>> act = () => System.CreateAsync(null);
                        act.ShouldThrow<ArgumentNullException>();
                    }
                }

                [TestFixture]
                public class Given_populated_model : When_creating_new_Client
                {
                    protected override void SetUp()
                    {
                        base.SetUp();

                        Result = System.CreateAsync(Model).Result;
                    }

                    protected IdentityServer4.Models.Client Model = new IdentityServer4.Models.Client
                    {
                        ClientId = "hello",
                        ClientSecrets = new List<Secret> {new Secret {Value = "world"}}
                    };

                    protected IdentityServer4.Models.Client Result;

                    [Test]
                    public void Then_database_changes_should_be_finalized()
                    {
                        A.CallTo(() => ConfigurationDbContext.SaveChangesAsync())
                            .MustHaveHappened();
                    }

                    [Test]
                    public void Then_model_should_be_loaded_into_the_database()
                    {
                        A.CallTo(() => MockDbSet.Add(A<Client>.That.Matches(x => x.ClientId == Model.ClientId)))
                            .MustHaveHappened();
                    }

                    [Test]
                    public void Then_return_original_object()
                    {
                        Result.ShouldBeSameAs(Model);
                    }
                }
            }

            [TestFixture]
            public class When_updating_an_existing_Client : Modification
            {
                [TestFixture]
                public class Given_null_model : When_updating_an_existing_Client
                {
                    [Test]
                    public void Then_throw_ArgumentNullException()
                    {
                        Func<Task<IdentityServer4.Models.Client>> act = () => System.UpdateAsync(null);
                        act.ShouldThrow<ArgumentNullException>();
                    }
                }

                [TestFixture]
                public class Given_populated_model : When_updating_an_existing_Client
                {
                    protected override void SetUp()
                    {
                        base.SetUp();

                        Result = System.UpdateAsync(Model).Result;
                    }

                    protected IdentityServer4.Models.Client Model = new IdentityServer4.Models.Client
                    {
                        ClientId = "hello",
                        ClientSecrets = new List<Secret> {new Secret {Value = "world"}}
                    };

                    protected IdentityServer4.Models.Client Result;

                    [Test]
                    public void Then_database_changes_should_be_finalized()
                    {
                        A.CallTo(() => ConfigurationDbContext.SaveChangesAsync())
                            .MustHaveHappened();
                    }

                    [Test]
                    public void Then_model_should_be_loaded_into_the_database()
                    {
                        A.CallTo(() => MockDbSet.Update(A<Client>.That.Matches(x => x.ClientId == Model.ClientId)))
                            .MustHaveHappened();
                    }

                    [Test]
                    public void Then_return_original_object()
                    {
                        Result.ShouldBeSameAs(Model);
                    }
                }
            }

            [TestFixture]
            public class When_deleting_by_integer_id : Modification
            {
                [Test]
                public void Then_it_is_not_implemented()
                {
                    Should.Throw<NotImplementedException>(() => { System.DeleteAsync(1); });
                }
            }

            [TestFixture]
            public class When_deleting_by_model : Modification
            {
                [TestFixture]
                public class Given_null_model : When_updating_an_existing_Client
                {
                    [Test]
                    public void Then_throw_ArgumentNullException()
                    {
                        Func<Task> act = () => System.DeleteAsync(null);
                        act.ShouldThrow<ArgumentNullException>();
                    }
                }

                [TestFixture]
                public class Given_populated_model : When_updating_an_existing_Client
                {
                    protected override void SetUp()
                    {
                        base.SetUp();

                        System.DeleteAsync(Model).Wait();
                    }

                    protected IdentityServer4.Models.Client Model = new IdentityServer4.Models.Client
                    {
                        ClientId = "hello",
                        ClientSecrets = new List<Secret> {new Secret {Value = "world"}}
                    };

                    protected IdentityServer4.Models.Client Result;

                    [Test]
                    public void Then_database_changes_should_be_finalized()
                    {
                        A.CallTo(() => ConfigurationDbContext.SaveChangesAsync())
                            .MustHaveHappened();
                    }

                    [Test]
                    public void Then_model_should_be_removed_from_the_database()
                    {
                        A.CallTo(() => MockDbSet.Remove(A<Client>.That.Matches(x => x.ClientId == Model.ClientId)))
                            .MustHaveHappened();
                    }
                }
            }
        }

        [TestFixture]
        public class Query : ClientRepositoryTests
        {
            [SetUp]
            protected override void SetUp()
            {
                base.SetUp();

                FakeDbSet = new FakeAsyncDbSet<Client>();
                A.CallTo(() => ConfigurationDbContext.Clients).Returns(FakeDbSet);
            }

            protected FakeAsyncDbSet<Client> FakeDbSet;

            [TestFixture]
            public class When_getting_all : Query
            {
                [TestFixture]
                public class Given_there_are_two_entities : When_getting_all
                {
                    [SetUp]
                    protected override void SetUp()
                    {
                        base.SetUp();

                        // Inject the two objects into the fake database
                        FakeDbSet.AddRange(Entity1, Entity2);

                        // Call the system under test
                        Result = System.GetAllAsync().Result;
                    }

                    protected Client Entity2 = new Client
                    {
                        ClientId = "hello"
                    };

                    protected Client Entity1 = new Client
                    {
                        ClientId = "world"
                    };

                    protected IReadOnlyList<IdentityServer4.Models.Client> Result;

                    [Test]
                    public void Then_result_should_contain_Entity1()
                    {
                        Result.FirstOrDefault(x => x.ClientId == Entity1.ClientId).ShouldNotBeNull();
                    }

                    [Test]
                    public void Then_result_should_contain_Entity2()
                    {
                        Result.FirstOrDefault(x => x.ClientId == Entity2.ClientId).ShouldNotBeNull();
                    }

                    [Test]
                    public void Then_result_should_contain_two_models()
                    {
                        Result.Count.ShouldBe(2);
                    }
                }

                [TestFixture]
                public class Given_there_are_no_entities : When_getting_all
                {
                    [SetUp]
                    protected override void SetUp()
                    {
                        base.SetUp();

                        Result = System.GetAllAsync().Result;
                    }

                    protected IReadOnlyList<IdentityServer4.Models.Client> Result;

                    [Test]
                    public void Then_result_should_contain_zero_models()
                    {
                        Result.Count.ShouldBe(0);
                    }
                }
            }

            [TestFixture]
            public class When_getting_one_by_clientId : Query
            {
                [TestFixture]
                public class Given_there_is_a_matching_entity : When_getting_one_by_clientId
                {
                    [SetUp]
                    protected override void SetUp()
                    {
                        base.SetUp();

                        // Inject the two objects into the fake database
                        FakeDbSet.AddRange(Entity1, Entity2);

                        // Call the system under test
                        Result = System.GetByClientIdAsync(Entity1.ClientId).Result;
                    }

                    protected Client Entity2 = new Client
                    {
                        ClientId = "hello"
                    };

                    protected Client Entity1 = new Client
                    {
                        ClientId = "world"
                    };

                    protected IdentityServer4.Models.Client Result;

                    [Test]
                    public void Then_result_should_match_Entity1()
                    {
                        Result.ClientId.ShouldBe(Entity1.ClientId);
                    }

                    [Test]
                    public void Then_result_should_not_be_null()
                    {
                        Result.ShouldNotBeNull();
                    }
                }

                [TestFixture]
                public class Given_there_is_no_matching_entity : When_getting_one_by_clientId
                {
                    [SetUp]
                    protected override void SetUp()
                    {
                        base.SetUp();

                        Result = System.GetByClientIdAsync("random").Result;
                    }

                    protected IdentityServer4.Models.Client Result;

                    [Test]
                    public void Then_result_should_be_null()
                    {
                        Result.ShouldBeNull();
                    }
                }
            }

            [TestFixture]
            public class When_getting_one_by_id : ClientRepositoryTests
            {
                [Test]
                public void Then_throw_NotImplementedException()
                {
                    // ReSharper disable once AssignmentIsFullyDiscarded
                    void Act()
                    {
                        _ = System.GetAsync(1).Result;
                    }

                    Should.Throw<NotImplementedException>(Act);
                }
            }
        }
    }
}