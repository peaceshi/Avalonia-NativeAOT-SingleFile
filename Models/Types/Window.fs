namespace Models.Types

type WindowSize =
    { MinWidth: Width
      MinHeight: Height
      Width: Width
      Height: Height
      MaxWidth: Width
      MaxHeight: Height }

    member this.WithWidth(x) =
        if x >= this.MinWidth then
            Ok { this with Width = x }
        else
            Error ""

    member this.WithHeight(y) = { this with Height = y }
    member this.WithMinWidth(x) = { this with MinWidth = x }
    member this.WithMinHeight(y) = { this with MinHeight = y }
    member this.WithMaxWidth(x) = { this with MaxWidth = x }
    member this.WithMaxHeight(y) = { this with MaxHeight = y }

type Window = { Size: WindowSize }
