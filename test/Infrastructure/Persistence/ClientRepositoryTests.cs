using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FakeItEasy;
using IdentityServer4.EntityFramework.Entities;
using IdentityServer4.EntityFramework.Interfaces;
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
        protected const string ClientId = "hello";

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

                MockDbSet = new FakeAsyncDbSet<Client>();
                A.CallTo(() => ConfigurationDbContext.Clients).Returns(MockDbSet);
            }

            protected FakeAsyncDbSet<Client> MockDbSet;

            [TestFixture]
            public class When_creating_new_Client : Modification
            {
                [TestFixture]
                public class Given_null_model : When_creating_new_Client
                {
                    [Test]
                    public void Then_throw_ArgumentNullException()
                    {
                        Func<Task<int>> act = () => System.CreateAsync(null);
                        act.ShouldThrow<ArgumentNullException>();
                    }
                }

                [TestFixture]
                public class Given_populated_model : When_creating_new_Client
                {
                    protected override void SetUp()
                    {
                        base.SetUp();

                        // Arrange
                        A.CallTo(() => ConfigurationDbContext.SaveChangesAsync())
                            .Returns(Task.FromResult(1));

                        // Act
                        Result = System.CreateAsync(Model).Result;
                    }

                    protected IdentityServer4.Models.Client Model = new IdentityServer4.Models.Client
                    {
                        ClientId = ClientId,
                        ClientSecrets = new List<Secret> { new Secret { Value = "world" } }
                    };

                    protected int Result;

                    [Test]
                    public void Then_database_changes_should_be_finalized()
                    {
                        A.CallTo(() => ConfigurationDbContext.SaveChangesAsync())
                            .MustHaveHappened();
                    }

                    [Test]
                    public void Then_model_should_be_loaded_into_the_database()
                    {
                        MockDbSet.Added.ShouldSatisfyAllConditions(
                            () => MockDbSet.Added.Count.ShouldBe(1),
                            () => MockDbSet.Added.ShouldContain(x => x.ClientId == ClientId)
                        );
                    }

                    [Test]
                    public void Then_record_count_is_one()
                    {
                        Result.ShouldBe(1);
                    }
                }
            }

            [TestFixture]
            public class When_updating_a_Client : Modification
            {
                [TestFixture]
                public class Given_null_model : When_updating_a_Client
                {
                    [Test]
                    public void Then_throw_ArgumentNullException()
                    {
                        Func<Task<int>> act = () => System.UpdateAsync(null);
                        act.ShouldThrow<ArgumentNullException>();
                    }
                }

                [TestFixture]
                public class Given_clientId_does_not_exist : When_updating_a_Client
                {
                    protected override void SetUp()
                    {
                        base.SetUp();

                        Result = System.UpdateAsync(Model).Result;
                    }

                    protected IdentityServer4.Models.Client Model = new IdentityServer4.Models.Client
                    {
                        ClientId = ClientId,
                        ClientSecrets = new List<Secret> { new Secret { Value = "world" } }
                    };

                    protected int Result;

                    [Test]
                    public void Then_result_count_should_be_zero()
                    {
                        Result.ShouldBe(0);
                    }
                }

                [TestFixture]
                public class Given_populated_model : When_updating_a_Client
                {
                    protected override void SetUp()
                    {
                        base.SetUp();

                        // Arrange
                        A.CallTo(() => ConfigurationDbContext.SaveChangesAsync())
                            .Returns(Task.FromResult(1));

                        MockDbSet.List.Add(_entity);

                        // Act
                        Result = System.UpdateAsync(_model).Result;
                    }


                    private const string _clientName = "world";

                    private readonly Client _entity = new Client
                    {
                        ClientId = ClientId
                    };

                    private readonly IdentityServer4.Models.Client _model = new IdentityServer4.Models.Client
                    {
                        ClientId = ClientId,
                        ClientName = _clientName
                    };

                    protected int Result;

                    [Test]
                    public void Then_clientName_should_be_changed()
                    {
                        _entity.ClientName.ShouldBe(_clientName);
                    }

                    [Test]
                    public void Then_database_changes_should_be_finalized()
                    {
                        A.CallTo(() => ConfigurationDbContext.SaveChangesAsync())
                            .MustHaveHappened();
                    }

                    [Test]
                    public void Then_entity_should_be_set_for_update()
                    {
                        MockDbSet.Updated.ShouldSatisfyAllConditions(
                            () => MockDbSet.Updated.Count.ShouldBe(1),
                            () => MockDbSet.Updated[0].ClientId.ShouldBe(ClientId),
                            () => MockDbSet.Updated[0].ClientName.ShouldBe(_clientName)
                        );
                    }

                    [Test]
                    public void Then_record_count_is_one()
                    {
                        Result.ShouldBe(1);
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
                public class Given_null_model : When_deleting_by_model
                {
                    [Test]
                    public void Then_throw_ArgumentNullException()
                    {
                        Func<Task<int>> act = () => System.DeleteAsync(null);
                        act.ShouldThrow<ArgumentNullException>();
                    }
                }

                [TestFixture]
                public class Given_client_exists : When_deleting_by_model
                {
                    private const int _id = 1234;

                    protected override void SetUp()
                    {
                        base.SetUp();

                        // Arrange
                        A.CallTo(() => ConfigurationDbContext.SaveChangesAsync())
                            .Returns(Task.FromResult(1));

                        MockDbSet.List.Add(_entity);

                        // Act
                        Result = System.DeleteAsync(_model).Result;
                    }

                    private readonly IdentityServer4.Models.Client _model = new IdentityServer4.Models.Client
                    {
                        ClientId = ClientId,
                        ClientSecrets = new List<Secret> { new Secret { Value = "world" } }
                    };

                    private readonly Client _entity = new Client
                    {
                        ClientId = ClientId,
                        Id = _id
                    };

                    protected int Result;

                    [Test]
                    public void Then_database_changes_should_be_finalized()
                    {
                        A.CallTo(() => ConfigurationDbContext.SaveChangesAsync())
                            .MustHaveHappened();
                    }

                    [Test]
                    public void Then_model_should_be_removed_from_the_database()
                    {
                        MockDbSet.Deleted.ShouldSatisfyAllConditions(
                            () => MockDbSet.Deleted.Count.ShouldBe(1),
                            () => MockDbSet.Deleted.ShouldContain(x => x.Id == _id)
                        );
                    }

                    [Test]
                    public void Then_record_count_is_one()
                    {
                        Result.ShouldBe(1);
                    }
                }

                [TestFixture]
                public class Given_client_does_not_exist : When_deleting_by_model
                {
                    // Behavior is not modified for this case. Tests created just for regression
                    // and to "document" that the result is simply passed back up the call stack.

                    protected override void SetUp()
                    {
                        base.SetUp();
                        
                        // Arrange
                        // note: not adding the entity to the fake DBSet object

                        // Act
                        Result = System.DeleteAsync(Model).Result;
                    }

                    protected IdentityServer4.Models.Client Model = new IdentityServer4.Models.Client
                    {
                        ClientId = ClientId
                    };

                    protected int Result;

                    [Test]
                    public void Then_record_count_is_zero()
                    {
                        Result.ShouldBe(0);
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
                        FakeDbSet.List.Add(Entity1);
                        FakeDbSet.List.Add(Entity2);

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
                        FakeDbSet.List.Add(Entity1);
                        FakeDbSet.List.Add(Entity2);

                        // Call the system under test
                        Result = System.GetByClientIdAsync(Entity1.ClientId).Result;
                    }

                    protected Client Entity2 = new Client
                    {
                        ClientId = ClientId
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