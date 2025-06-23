using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Drawing;
using Media.Control.App.ManagerBa.ViewModel;
using Media.Control.App.ManagerBa.Model.HotKey;
using Media.Control.App.ManagerBa.Model;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Windows.Media.Animation;

namespace Media.Control.App.ManagerBa.View
{
    /// <summary>
    /// ConfigWindowView.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ConfigWindowView : Window
    {

        [DllImport("user32.dll")]
        private static extern short GetAsyncKeyState(int vKey);

        private HotKeyDefin _selectedItem;
        private DataGridCellInfo _selectedCell;
        public double GridRowHeigth = 0;

        private ConfigWindowViewModel configWindowViewModel = null;
        public ConfigWindowView()
        {
            InitializeComponent();

            AdjustWindowHeight();
            configWindowViewModel = new ConfigWindowViewModel(this);

            this.DataContext = configWindowViewModel;
            this.WindowStartupLocation = WindowStartupLocation.Manual;
            this.Top = 47;  // 아래에서 200px 위치
            this.Left = SystemParameters.WorkArea.Width - this.Width - 2;

            
        }
        private void AdjustWindowHeight()
        {
            // 화면 크기 가져오기
            var screenHeight = SystemParameters.PrimaryScreenHeight;

            // 화면 크기에 따라 창 높이 설정 (예: 화면 높이의 70%)
            this.Height = screenHeight - 50;
            GridRowHeigth = this.Height - 40;
            // 중앙에 배치
            this.Top = (screenHeight - this.Height) / 2;
        }

        private bool GetKeyValue(StringBuilder key)
        {
            var keys = configWindowViewModel.HotKeyDefinList.Where(c => c.Value == key.ToString()).ToList();

            return keys.Any();
        }

        private void DataGrid_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            //if (_selectedItem != null && _selectedCell.Column.Header.ToString() == "KeyCombination")

            if (_selectedItem != null && _selectedCell.Column.Header.ToString() == "Value")
            {
                StringBuilder keyCombination = new StringBuilder();

                bool sysKey = false;
                // 조합키 감지
                if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
                {
                    keyCombination.Append("Ctrl+");
                    sysKey = true;
                }
                if (Keyboard.IsKeyDown(Key.LeftAlt) || Keyboard.IsKeyDown(Key.RightAlt))
                {
                    keyCombination.Append("Alt+");
                    sysKey = true;
                }
                if (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift))
                {
                    keyCombination.Append("Shift+");
                    sysKey = true;
                }

                // 현재 눌린 키 확인 (조합키 제외)
                if (e.Key != Key.LeftCtrl && e.Key != Key.RightCtrl &&
                    e.Key != Key.LeftAlt && e.Key != Key.RightAlt &&
                    e.Key != Key.LeftShift && e.Key != Key.RightShift &&
                    e.Key != Key.System && e.Key != Key.None)
                {
                    keyCombination.Append(e.Key.ToString());
                }


                if(!GetKeyValue(keyCombination))
                    _selectedItem.Value = keyCombination.ToString();
                else
                    _selectedItem.Value = "None";

                if (!sysKey)
                {
                    // DataGrid의 트랜잭션 종료
                    var dataGrid = sender as DataGrid;
                    if (dataGrid != null)
                    {
                        dataGrid.CommitEdit();  // 현재 편집 트랜잭션 종료
                        dataGrid.CancelEdit();  // 모든 트랜잭션 종료
                    }

                    HotkeyGridList.Items.Refresh();
                }

                // 이벤트 처리 완료
                e.Handled = true;
            }
        }

        private void DataGrid_SelectedCellsChanged(object sender, SelectedCellsChangedEventArgs e)
        {
            // 선택된 셀 및 항목 추적
            _selectedCell = HotkeyGridList.CurrentCell;

            if (HotkeyGridList.SelectedItem is HotKeyDefin selectedItem)
            {
                _selectedItem = selectedItem;
            }
        }

        private void HotkeyGridList_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (_selectedItem != null && _selectedCell.Column.Header.ToString() == "KeyCombination")
            {
                // 문자 입력 추가
                _selectedItem.KeyCombination += e.Text;

                // DataGrid 업데이트
                HotkeyGridList.Items.Refresh();

                // 이벤트 처리 완료
                e.Handled = true;
            }
        }

        private void HotkeyGridList_PreviewKeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            // DataGrid의 트랜잭션 종료
            var dataGrid = sender as DataGrid;
            if (dataGrid != null)
            {
                dataGrid.CommitEdit();  // 현재 편집 트랜잭션 종료
                dataGrid.CancelEdit();  // 모든 트랜잭션 종료
            }

            HotkeyGridList.Items.Refresh();
            // 이벤트 처리 완료
            e.Handled = true;

        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {

                var selectedItem = HotkeyGridList.SelectedItem as HotKeyDefin;
                if (selectedItem != null)
                {   
                    HotkeyGridList.CommitEdit();  // 현재 편집 트랜잭션 종료
                    HotkeyGridList.CancelEdit();  // 모든 트랜잭션 종료
                 
                    // 특정 열 값 지우기 (예: Address 열)
                    selectedItem.Value = "None";

                    // UI 새로고침
                    HotkeyGridList.Items.Refresh();
                }

            }
            catch { }

            //configWindowViewModel.CommandKeyClean(e);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // 초기 위치 설정 (화면의 오른쪽 바깥)
            var screenWidth = SystemParameters.PrimaryScreenWidth;
            this.Left = screenWidth; // 창을 화면 밖으로 이동 (오른쪽)
            //this.Top = (SystemParameters.PrimaryScreenHeight - this.Height) / 2; // 세로 중앙 정렬

            // 애니메이션 정의
            DoubleAnimation slideAnimation = new DoubleAnimation
            {
                From = screenWidth, // 시작 위치 (오른쪽 화면 밖)
                To = screenWidth - this.Width, // 끝 위치 (오른쪽 화면 끝)
                Duration = TimeSpan.FromSeconds(1.5), // 애니메이션 시간
                EasingFunction = new QuinticEase { EasingMode = EasingMode.EaseOut } // 부드러운 애니메이션
            };

            // 애니메이션 시작
            this.BeginAnimation(Window.LeftProperty, slideAnimation);
        }

        private bool _isClosing = false;
        private void butClose_Click(object sender, RoutedEventArgs e)
        {
            SetClose();
        }


        public void SetClose()
        {
            var screenWidth = SystemParameters.PrimaryScreenWidth;

            // 슬라이드 아웃 애니메이션 생성
            DoubleAnimation slideOutAnimation = new DoubleAnimation
            {
                From = this.Left,
                To = screenWidth, // 창을 오른쪽 밖으로 이동
                Duration = TimeSpan.FromSeconds(0.8),
                EasingFunction = new QuinticEase { EasingMode = EasingMode.EaseIn }
            };

            // 애니메이션 완료 이벤트 등록
            slideOutAnimation.Completed += SlideOutAnimation_Completed;

            this.BeginAnimation(Window.LeftProperty, slideOutAnimation);
        }


        private void SlideOutAnimation_Completed(object sender, EventArgs e)
        {
            // 애니메이션 완료 후 정리 및 창 닫기
            Dispatcher.BeginInvoke(new Action(() =>
            {
                _isClosing = false; // 닫기 중 상태 초기화
                this.BeginAnimation(Window.LeftProperty, null); // 애니메이션 해제
                this.Close(); // 창 닫기
            }));

            // 애니메이션 핸들러 제거
            if (sender is AnimationClock animationClock)
            {
                animationClock.Completed -= SlideOutAnimation_Completed;
            }
        }

        private void ComEngineType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //var comboBox = sender as ComboBox;
            //var selected = comboBox?.SelectedItem;

        }
            
    }
}
