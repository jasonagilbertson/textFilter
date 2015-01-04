namespace RegexViewer
{
    public class FilterTabViewModel : BaseTabViewModel<FilterFileItem>
    {
        #region Public Constructors

        //private Command _newItemCommand;
        public FilterTabViewModel()
        {
        }

        #endregion Public Constructors

        // public void NewItemExecuted(object sender) { SetStatus("NewItemExecuted:enter"); //
        // 141227 //
        // http: //blogs.msdn.com/b/vinsibal/archive/2008/11/05/wpf-datagrid-new-item-template-sample.aspx
        // try { if (sender is DataGrid) //if (sender is ItemCollection) { DataGrid grid = (sender
        // as DataGrid);

        // FilterFileItem fileItem = (FilterFileItem)(sender as ItemCollection).CurrentItem; foreach
        // (var item in (sender as ItemCollection)) { SetStatus(item.ToString()); if (!(item is
        // FilterFileItem) ) { // remove temp entry // DataGrid.DeleteCommand } }
        // //IEnumerable<FilterFileItem> items = (sender as ItemCollection).Cast<FilterFileItem>();
        // //FilterFileItem item = (FilterFileItem)(items.Last()); //item.Index =
        // this.ContentList.Max(x => x.Index) + 1; // FilterFileItem fileItem = new
        // FilterFileItem(); //fileItem.Index = this.ContentList.Max(x => x.Index) + 1;
        // //grid.Items.Add(fileItem); // grid.SelectedIndex = grid.Items.Count - 1; // this.ContentList.Add(fileItem);

        // //t.Index = (IFileItem)(sender as ItemCollection).Cast<T>().Max(x => x.Index) + 1;

        // // ((IFileItem)t[t.Count - 1]).Index = 1; } } catch (Exception ex) {
        // SetStatus("NewItem:exception" + ex.ToString()); } }

        // public Command NewItemCommand { get { if (_newItemCommand == null) { _newItemCommand =
        // new Command(NewItemExecuted); } _newItemCommand.CanExecute = true;

        // return _newItemCommand; } set { _newItemCommand = value; } }
    }
}