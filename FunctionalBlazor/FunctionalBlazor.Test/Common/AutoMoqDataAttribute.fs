namespace FunctionalBlazor.Test.Common

open AutoFixture.NUnit3
open AutoFixture
open AutoFixture.AutoMoq

type AutoMoqDataAttribute () =
    inherit AutoDataAttribute (fun () -> Fixture().Customize(new AutoMoqCustomization(ConfigureMembers = true)))
