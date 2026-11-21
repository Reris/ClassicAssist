using Avalonia;
using Avalonia.Controls;

namespace ClassicAssist.UI.Misc
{
    /// <summary>
    ///     Supplies the <see cref="Enums" /> sort-field discriminator used by <see cref="SkillComparer" />
    ///     and an attached property for tagging <see cref="DataGridColumn" />s with their sort field.
    /// </summary>
    public class SkillsGridViewColumn
    {
        public enum Enums
        {
            Name,
            Value,
            Base,
            Delta,
            Cap,
            LockStatus
        }

        public static readonly AttachedProperty<Enums> SortFieldProperty =
            AvaloniaProperty.RegisterAttached<SkillsGridViewColumn, DataGridColumn, Enums>( "SortField",
                Enums.Name );

        public static Enums GetSortField( DataGridColumn column )
        {
            return column.GetValue( SortFieldProperty );
        }

        public static void SetSortField( DataGridColumn column, Enums value )
        {
            column.SetValue( SortFieldProperty, value );
        }
    }
}