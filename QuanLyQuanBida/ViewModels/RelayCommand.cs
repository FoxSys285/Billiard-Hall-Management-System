using System;
using System.Windows.Controls;
using System.Windows.Input;

// RelayCommand là class trung gian giúp View gọi hàm trong ViewModel
public class RelayCommand : ICommand
{
    // Hàm sẽ được thực thi khi Command chạy (ví dụ: click Button)
    private readonly Action<object> _execute;

    // Hàm kiểm tra điều kiện cho phép Command chạy
    // Nếu trả về false → Button sẽ bị disable
    private readonly Predicate<object> _canExecute;

    // Constructor: nhận hàm Execute và hàm CanExecute từ ViewModel
    public RelayCommand(Action<object> execute, Predicate<object> canExecute = null)
    {
        // Kiểm tra execute không được null
        // vì Command bắt buộc phải có hành động để chạy
        _execute = execute ?? throw new ArgumentNullException(nameof(execute)); //??: toán tử thay thế khi null

        // CanExecute có thể null (nghĩa là luôn cho phép chạy)
        _canExecute = canExecute;
    }

    // WPF gọi hàm này để kiểm tra Button có được click hay không; quyết định bật/tắt Button; true  → Button ENABLE; false → Button DISABLE
    public bool CanExecute(object parameter)
    {
        // Nếu không có điều kiện thì luôn cho phép chạy
        if (_canExecute == null)
            return true;

        // Nếu có điều kiện thì gọi hàm kiểm tra
        return _canExecute(parameter);
    }

    // WPF gọi hàm này khi user thực hiện Command (ví dụ click Button)
    public void Execute(object parameter)
    {
        // Gọi hàm được truyền từ ViewModel
        _execute(parameter);
    }

    // Event giúp UI cập nhật lại trạng thái Enable/Disable của Button
    //Dùng để WPF đăng ký lắng nghe: Khi nào trạng thái thay đổi thì báo cho UI biết --> người nghe
    public event EventHandler CanExecuteChanged;

    // Hàm dùng để thông báo cho UI trạng thái có thể đã thay đổi, kiểm tra lại CanExecute --> người báo tin
    public void RaiseCanExecuteChanged()
    {
        // ?. là null-safe operator - toán tử kiểm tra null trước khi truy cập
       CanExecuteChanged?.Invoke(this, EventArgs.Empty); 
    }
}