using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FakeItEasy;
using IdentityServer4.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using NUnit.Framework;
using safnet.Identity.Api.Infrastructure.Persistence;
using safnet.Identity.Api.Services.Controllers;
using safnet.Identity.Api.Services.Models;
using Shouldly;

// ReSharper disable InconsistentNaming

namespace safnet.Identity.Api.UnitTests.Services.Controllers
{
    [TestFixture]
    public class ClientsControllerTests
    {
        [SetUp]
        protected virtual void SetUp()
        {
            ClientRepository = A.Fake<IClientRepository>();
            System = new ClientsController(ClientRepository);
        }

        protected ClientsController System;
        protected IClientRepository ClientRepository;

        [TestFixture]
        public class When_getting_all_clients : ClientsControllerTests
        {
            [TestFixture]
            public class Given_there_are_no_clients : When_getting_all_clients
            {
                protected override void SetUp()
                {
                    base.SetUp();

                    // Arrange
                    A.CallTo(() => ClientRepository.GetAllAsync())
                        .Returns(Task.FromResult(new List<Client>() as IReadOnlyList<Client>));

                    // Act
                    ActionResult = System.Get().Result as OkObjectResult;
                }

                protected OkObjectResult ActionResult;

                [Test]
                public void Then_respond_with_status_200()
                {
                    ActionResult.ShouldNotBeNull();
                }

                [Test]
                public void Then_result_is_an_empty_array()
                {
                    var result = ActionResult.Value as IEnumerable<Client>;
                    result.ShouldNotBeNull();
                }
            }

            [TestFixture]
            public class Given_there_are_two_clients : When_getting_all_clients
            {
                protected override void SetUp()
                {
                    base.SetUp();

                    // Arrange
                    A.CallTo(() => ClientRepository.GetAllAsync())
                        .Returns(Task.FromResult(new[] { Model1, Model2 } as IReadOnlyList<Client>));

                    // Act
                    ActionResult = System.Get().Result as OkObjectResult;
                }

                protected Client Model1 = new Client();
                protected Client Model2 = new Client();

                protected OkObjectResult ActionResult;

                [Test]
                public void Then_respond_with_status_200()
                {
                    ActionResult.ShouldNotBeNull();
                }

                [Test]
                public void Then_result_contains_both_objects()
                {
                    var result = ActionResult.Value as IEnumerable<Client>;

                    result.ShouldSatisfyAllConditions(
                        () => result.ShouldNotBeNull(),
                        () => result.ShouldContain(Model1),
                        () => result.ShouldContain(Model2)
                    );
                }
            }

            // TODO: consider null response handling
        }

        [TestFixture]
        public class When_getting_one_client : ClientsControllerTests
        {
            protected const string ClientId = "asdfasdfasdf";

            [TestFixture]
            public class Given_the_client_exists : When_getting_one_client
            {
                protected override void SetUp()
                {
                    base.SetUp();

                    // Arrange
                    A.CallTo(() => ClientRepository.GetByClientIdAsync(ClientId))
                        .Returns(Task.FromResult(Model));

                    // Act
                    ActionResult = System.Get(ClientId).Result as OkObjectResult;
                }

                protected OkObjectResult ActionResult;

                protected Client Model = new Client();

                [Test]
                public void Then_respond_with_status_200()
                {
                    ActionResult.ShouldNotBeNull();
                }

                [Test]
                public void Then_result_is_the_expected_model()
                {
                    var result = ActionResult.Value as Client;

                    result.ShouldSatisfyAllConditions(
                        () => result.ShouldNotBeNull(),
                        () => result.ShouldBeSameAs(Model)
                    );
                }
            }

            [TestFixture]
            public class Given_the_client_does_not_exist : When_getting_one_client
            {
                protected override void SetUp()
                {
                    base.SetUp();

                    // Arrange
                    A.CallTo(() => ClientRepository.GetByClientIdAsync(ClientId))
                        .Returns(Task.FromResult(null as Client));

                    // Act
                    ActionResult = System.Get(ClientId).Result as NotFoundObjectResult;
                }

                protected NotFoundObjectResult ActionResult;


                [Test]
                public void Then_respond_with_status_404()
                {
                    ActionResult.ShouldNotBeNull();
                }
            }
        }

        [TestFixture]
        public class When_creating_a_new_client : ClientsControllerTests
        {
            [TestFixture]
            public class Given_valid_model : When_creating_a_new_client
            {
                [SetUp]
                protected override void SetUp()
                {
                    base.SetUp();

                    A.CallTo(() => ClientRepository.GetByClientIdAsync(ClientId))
                        .Returns(Task.FromResult(null as Client));

                    Result = System.Post(_model).Result as CreatedAtRouteResult;
                }

                private const string ClientId = "edsafg";

                private readonly Client _model = new Client { ClientId = ClientId };

                protected CreatedAtRouteResult Result;

                [Test]
                public void Then_respond_with_clientId_in_route_information_for_location_header()
                {
                    Result.ShouldSatisfyAllConditions(
                        () => Result.Value.ShouldBeAssignableTo<Client>(),
                        () => ((Client)Result.Value).ClientId.ShouldBe(ClientId)
                    );
                }

                [Test]
                public void Then_respond_with_route_name_for_location_header()
                {
                    Result.RouteName.ShouldBe(ClientsController.ClientIdRouteName);
                }

                [Test]
                public void Then_respond_with_status_code_201()
                {
                    Result.ShouldNotBeNull();
                }

                [Test]
                public void Then_should_persist_the_object()
                {
                    A.CallTo(() => ClientRepository.CreateAsync(_model)).MustHaveHappenedOnceExactly();
                }
            }

            [TestFixture]
            public class Given_invalid_model : When_creating_a_new_client
            {
                protected const string ErrorKey = "ClientId";
                protected const string ErrorValue = "Required";

                [SetUp]
                protected override void SetUp()
                {
                    base.SetUp();

                    // Arrange
                    System.ModelState.AddModelError(ErrorKey, ErrorValue);

                    // Act
                    Result = System.Post(_model).Result as BadRequestObjectResult;
                }

                private readonly Client _model = new Client();

                protected BadRequestObjectResult Result;

                [Test]
                public void Then_respond_with_400()
                {
                    Result.ShouldNotBeNull();
                }

                [Test]
                public void Then_describe_problem_in_response()
                {
                    Result.ShouldSatisfyAllConditions(
                        () => Result.Value.ShouldBeAssignableTo<SerializableError>(),
                        () => ((SerializableError)Result.Value).ContainsKey(ErrorKey).ShouldBeTrue(),
                        () => ((SerializableError)Result.Value)[ErrorKey].ShouldBeAssignableTo<string[]>(),
                        () => (((SerializableError)Result.Value)[ErrorKey] as string[])?[0].ShouldBe(ErrorValue)
                    );
                }

                [Test]
                public void Then_should_not_check_if_client_id_exists_already()
                {
                    A.CallTo(() => ClientRepository.GetByClientIdAsync(A<string>._))
                        .MustNotHaveHappened();
                }
            }

            [TestFixture]
            public class Given_client_id_already_exists : When_creating_a_new_client
            {
                [SetUp]
                protected override void SetUp()
                {
                    base.SetUp();

                    A.CallTo(() => ClientRepository.GetByClientIdAsync(ClientId))
                        .Returns(Task.FromResult(new Client()));

                    Result = System.Post(_model).Result as ConflictObjectResult;
                }

                private const string ClientId = "edsafg";

                private readonly Client _model = new Client { ClientId = ClientId };

                protected ConflictObjectResult Result;
                
                [Test]
                public void Then_respond_with_status_code_409()
                {
                    Result.ShouldNotBeNull();
                }

                [Test]
                public void Then_response_message_should_describe_the_problem()
                {
                    (Result.Value as MessageModel)?.Message.ShouldBe(ClientsController.ErrorMessageClientAlreadyExists);
                }

            }
        }

        [TestFixture]
        public class When_updating_a_client : ClientsControllerTests
        {
            protected const string ClientId = "asdf";

            protected virtual int ResultCount { get; } = 99;

            private readonly Client _model = new Client();

            protected override void SetUp()
            {
                base.SetUp();

                // Arrange
                A.CallTo(() => ClientRepository.UpdateAsync(_model))
                    .Returns(Task.FromResult(ResultCount));
            }

            [TestFixture]
            public class Given_client_does_not_exist : When_updating_a_client
            {
                protected override int ResultCount => 0;

                protected override void SetUp()
                {
                    base.SetUp();

                    Result = System.Put(ClientId, _model).Result as NotFoundResult;
                }

                protected NotFoundResult Result;

                [Test]
                public void Then_respond_with_status_code_400()
                {  
                    Result.ShouldNotBeNull();
                }
            }

            [TestFixture]
            public class Given_valid_model : When_updating_a_client
            {
                protected override int ResultCount => 1;

                protected override void SetUp()
                {
                    base.SetUp();

                    // Act
                    Result = System.Put(ClientId, _model).Result as AcceptedResult;
                }
                
                protected AcceptedResult Result;

                [Test]
                public void Then_respond_with_status_code_202()
                {
                    Result.ShouldNotBeNull();
                }

            }

            [TestFixture]
            public class Given_invalid_model : When_updating_a_client
            {
                protected const string ErrorKey = "ClientSecret";
                protected const string ErrorValue = "Required";

                protected override int ResultCount => 0;

                [SetUp]
                protected override void SetUp()
                {
                    base.SetUp();

                    // Arrange
                    System.ModelState.AddModelError(ErrorKey, ErrorValue);

                    // Act
                    Result = System.Put(ClientId, _model).Result as BadRequestObjectResult;
                }

                protected BadRequestObjectResult Result;

                [Test]
                public void Then_respond_with_400()
                {
                    Result.ShouldNotBeNull();
                }

                [Test]
                public void Then_describe_problem_in_response()
                {
                    Result.ShouldSatisfyAllConditions(
                        () => Result.Value.ShouldBeAssignableTo<SerializableError>(),
                        () => ((SerializableError)Result.Value).ContainsKey(ErrorKey).ShouldBeTrue(),
                        () => ((SerializableError)Result.Value)[ErrorKey].ShouldBeAssignableTo<string[]>(),
                        () => (((SerializableError)Result.Value)[ErrorKey] as string[])?[0].ShouldBe(ErrorValue)
                    );
                }

            }
        }

        [TestFixture]
        public class When_deleting_a_client : ClientsControllerTests
        {
            protected const string ClientId = "asdf";

            protected virtual int ResultCount { get; } = 99;
            
            protected override void SetUp()
            {
                base.SetUp();

                // Arrange
                A.CallTo(() => ClientRepository.DeleteAsync(A<Client>.That.Matches(x => x.ClientId == ClientId)))
                    .Returns(Task.FromResult(ResultCount));
            }

            [TestFixture]
            public class Given_client_does_not_exist : When_deleting_a_client
            {
                protected override int ResultCount => 0;

                protected override void SetUp()
                {
                    base.SetUp();

                    Result = System.Delete(ClientId).Result as NotFoundResult;
                }

                protected NotFoundResult Result;

                [Test]
                public void Then_respond_with_status_code_400()
                {
                    Result.ShouldNotBeNull();
                }
            }

            [TestFixture]
            public class Given_client_exists : When_deleting_a_client
            {
                protected override int ResultCount => 1;

                protected override void SetUp()
                {
                    base.SetUp();

                    // Act
                    Result = System.Delete(ClientId).Result as AcceptedResult;
                }

                protected AcceptedResult Result;

                [Test]
                public void Then_respond_with_status_code_202()
                {
                    Result.ShouldNotBeNull();
                }

            }
        }

        [TestFixture]
        public class When_passing_null_to_constructor : ClientsControllerTests
        {
            [Test]
            public void Then_throw_ArgumentNullException()
            {
                void Act()
                {
                    // ReSharper disable once ObjectCreationAsStatement
                    new ClientsController(null);
                }

                Should.Throw<ArgumentNullException>(Act);
            }
        }
    }
}