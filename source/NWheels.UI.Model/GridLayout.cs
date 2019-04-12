using System;

namespace NWheels.UI.Model
{
    public class GridLayoutProps : PropsOf<GridLayout>
    {
        public GridLayoutProps Cols(int count) => default;
        public GridLayoutProps Rows(int count) => default;
        public GridLayoutProps Cell(int row, int col, UIComponent component) => default;
        public GridLayoutProps Matrix(UIComponent[,] cells) => default;
    }
    
    public class GridLayout : LayoutComponent<GridLayoutProps, Empty.State>
    {
        public GridLayout(Action<GridLayoutProps> setProps)
        {
        }

        public GridLayout(UIComponent[,] matrix)
        {
        }
    }
}
