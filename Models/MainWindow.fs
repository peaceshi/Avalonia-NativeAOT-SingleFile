[<AutoOpen>]
module Models.MainWindow

open Models.Types
open Models.Constants

let Size =
    { Width = WindowDefaultWidth
      Height = WindowDefaultHeight
      MinWidth = WindowMinWidth
      MinHeight = WindowMinHeight
      MaxWidth = WindowMaxWidth
      MaxHeight = WindowMaxHeight }

let MainWindow = { Size = Size }
