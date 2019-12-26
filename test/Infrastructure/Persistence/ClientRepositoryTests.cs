using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FakeItEasy;
using IdentityServer4.EntityFramework.Interfaces;
using Entities=IdentityServer4.EntityFramework.Entities;
using IdentityServer4.Models;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using safnet.Identity.Api.Infrastructure.Persistence;
using Shouldly;

// ReSharper disable InconsistentNaming

namespace safnet.Identity.Api.UnitTests.Infrastructure.Persistence
{
    [TestFixture]
    public class ClientRepositoryTests
    {
        protected ClientRepository System;
        protected IConfigurationDbContext ConfigurationDbContext;
        
        [SetUp]
        protected virtual void SetUp()
        {
            ConfigurationDbContext = A.Fake<IConfigurationDbContext>();

            System = new ClientRepository(ConfigurationDbContext);
        }

        [TestFixture]
        public class Modification : ClientRepositoryTests
        {
            protected DbSet<Entities.Client> MockDbSet;
            
            [SetUp]
            protected override void SetUp()
            {
                base.SetUp();

                MockDbSet = A.Fake<DbSet<Entities.Client>>();
                A.CallTo(() => ConfigurationDbContext.Clients).Returns(MockDbSet);
            }

            [TestFixture]
            public class When_creating_new_Client : Modification
            {
                [TestFixture]
                public class Given_null_model : When_creating_new_Client
                {
                    [Test]
                    public void Then_throw_ArgumentNullException()
                    {
                        Func<Task<Client>> act = () => System.CreateAsync(null);
                        act.ShouldThrow<ArgumentNullException>();
                    }
                }

                [TestFixture]
                public class Given_populated_model : When_creating_new_Client
                {
                    protected Client Model = new Client
                    {
                        ClientId = "hello",
                        ClientSecrets = new List<Secret> { new Secret { Value = "world" } }
                    };
                    protected Client Result;

                    protected override void SetUp()
                    {
                        base.SetUp();

                        Result = System.CreateAsync(Model).Result;
                    }

                    [Test]
                    public void Then_return_original_object()
                    {
                        Result.ShouldBeSameAs(Model);
                    }

                    [Test]
                    public void Then_model_should_be_loaded_into_the_database()
                    {
                        A.CallTo(() => MockDbSet.Add(A<Entities.Client>.That.Matches(x => x.ClientId == Model.ClientId))).MustHaveHappened();
                    }

                    [Test]
                    public void Then_database_changes_should_be_finalized()
                    {
                        A.CallTo(() => ConfigurationDbContext.SaveChangesAsync())
                            .MustHaveHappened();
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
                        Func<Task<Client>> act = () => System.UpdateAsync(null);
                        act.ShouldThrow<ArgumentNullException>();
                    }
                }

                [TestFixture]
                public class Given_populated_model : When_updating_an_existing_Client
                {
                    protected Client Model = new Client
                    {
                        ClientId = "hello",
                        ClientSecrets = new List<Secret> { new Secret { Value = "world" } }
                    };
                    protected Client Result;

                    protected override void SetUp()
                    {
                        base.SetUp();

                        Result = System.UpdateAsync(Model).Result;
                    }

                    [Test]
                    public void Then_return_original_object()
                    {
                        Result.ShouldBeSameAs(Model);
                    }

                    [Test]
                    public void Then_model_should_be_loaded_into_the_database()
                    {
                        A.CallTo(() => MockDbSet.Update(A<Entities.Client>.That.Matches(x => x.ClientId == Model.ClientId))).MustHaveHappened();
                    }

                    [Test]
                    public void Then_database_changes_should_be_finalized()
                    {
                        A.CallTo(() => ConfigurationDbContext.SaveChangesAsync())
                            .MustHaveHappened();
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
                    protected Client Model = new Client
                    {
                        ClientId = "hello",
                        ClientSecrets = new List<Secret> { new Secret { Value = "world" } }
                    };
                    protected Client Result;

                    protected override void SetUp()
                    {
                        base.SetUp();

                        System.DeleteAsync(Model).Wait();
                    }

                    [Test]
                    public void Then_model_should_be_removed_from_the_database()
                    {
                        A.CallTo(() => MockDbSet.Remove(A<Entities.Client>.That.Matches(x => x.ClientId == Model.ClientId))).MustHaveHappened();
                    }

                    [Test]
                    public void Then_database_changes_should_be_finalized()
                    {
                        A.CallTo(() => ConfigurationDbContext.SaveChangesAsync())
                            .MustHaveHappened();
                    }
                }
            }

        }

        [TestFixture]
        public class When_getting_all_models : ClientRepositoryTests
        {
            [TestFixture]
            public class Given_there_are_two_entities : When_getting_all_models
            {

                protected Entities.Client Entity2 = new Entities.Client
                {
                    ClientId = "hello"
                };

                protected Entities.Client Entity1 = new Entities.Client
                {
                    ClientId = "world"
                };

                protected IReadOnlyList<Client> Result;

                [SetUp]
                protected override void SetUp()
                {
                    base.SetUp();

                    QueryableDbSet.AddRange(Entity1, Entity2);

                    Result = System.GetAllAsync().Result;
                }

                [Test]
                public void Then_result_should_contain_two_models()
                {
                    Result.Count.ShouldBe(2);
                }

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
            }
        }
    }
}
