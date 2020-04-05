namespace FunctionalBlazor.Test.Common

open AutoFixture.NUnit3
open AutoFixture
open AutoFixture.AutoMoq
open System

type AutoMoqInlineDataAttribute ([<ParamArray>]arguments : obj[]) =
    inherit InlineAutoDataAttribute (fun () -> Fixture().Customize(new AutoMoqCustomization(ConfigureMembers = true)), arguments)