using System.Diagnostics;
using System.Threading;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Graphics;


namespace PLANET_extra
{
	public partial class MainPage : ContentPage
	{

		// settings
		const int FIELD_SIZE = 10;
		private readonly Color EMPTY_FIELD_COLOR = Colors.Gray;
		private readonly Color OCCUPIED_FIELD_COLOR = Colors.Red;
		private readonly Color FIELD_BORDER_COLOR = Colors.LightGray;

		// variables
		List<bool[,]> moves = new List<bool[,]>();
		BoxView[,] arena = null;
		private Timer timer;
		bool timer_result = true;

		const int WIDTH = FIELD_SIZE;
		const int HEIGHT = FIELD_SIZE;

		int currentFrame = 0;
		int arenaSizeX = 2;
		int arenaSizeY = 2;

		// functions
		public MainPage()
		{
			InitializeComponent();
			Trace.WriteLine("Created main window.");
		}
		private void arena_MouseLeftButtonDown(BoxView sender)
		{
			Trace.WriteLine("pressed");
			Trace.WriteLine("x: " + Arena.GetColumn(sender));
			Trace.WriteLine("y: " + Arena.GetRow(sender));
	
			FieldClick(Arena.GetColumn(sender), Arena.GetRow(sender));

		}
		private void FieldClick(in int x, in int y)
		{
			var currentMove = moves[currentFrame];
			if (currentMove[x, y] == false)
			{
				currentMove[x, y] = true;
				arena[x, y].Color = OCCUPIED_FIELD_COLOR;
			}
			else
			{
				currentMove[x, y] = false;
				arena[x, y].Color = EMPTY_FIELD_COLOR;
			}
		}
		private void CreateArena_Click(object sender, EventArgs e)
		{
			SetInputSizeX();
			SetInputSizeY();

			CreateFields();
			CreateArena.IsEnabled = false;
			NextFrameButton.IsEnabled = true;
			StartButton.IsEnabled = true;
			LoadButton.IsEnabled = false;
			SaveButton.IsEnabled = true;

			Trace.WriteLine("Created arena. Button IsEnabled=false");
		}
		private void CreateFields()
		{
			moves.Add(new bool[arenaSizeX, arenaSizeY]);

			Arena.HeightRequest = arenaSizeY * (HEIGHT + 1);
			Arena.WidthRequest = arenaSizeX * (WIDTH + 1);

			Arena.RowDefinitions.Add(new RowDefinition { Height = new GridLength(arenaSizeY, GridUnitType.Star) });
			Arena.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(arenaSizeX, GridUnitType.Star) });

			arena = new BoxView[arenaSizeX, arenaSizeY];
			for (int y = 0; y < arenaSizeY; y++)
			{
				for (int x = 0; x < arenaSizeX; x++)
				{
					var field = new BoxView
					{
						WidthRequest = WIDTH,
						HeightRequest = HEIGHT,
						Color = EMPTY_FIELD_COLOR,
						TranslationX = -(Arena.WidthRequest - WIDTH) / 2		+ x * (WIDTH + 1),
						TranslationY = -(Arena.HeightRequest - HEIGHT) / 2		+ y * (HEIGHT + 1)
					};

					var tapGestureRecognizer = new TapGestureRecognizer();
					tapGestureRecognizer.Tapped += (s, e) => arena_MouseLeftButtonDown(field);
					field.GestureRecognizers.Add(tapGestureRecognizer);

					Arena.SetColumn(field, x);
					Arena.SetRow(field, y);
					Arena.Children.Add(field);

					arena[x, y] = field;
				}
			}

		}
		private void SetInputSizeX()
		{
			try
			{
				int.TryParse(InputSizeX.Text.ToString(), out arenaSizeX);
			}
			catch
			{
				arenaSizeX = 10;
			}
		}
		private void SetInputSizeY()
		{
			try
			{
				int.TryParse(InputSizeY.Text.ToString(), out arenaSizeY);
			}
			catch
			{
				arenaSizeY = 10;
			}
		}
		private void Start_Click(object sender, EventArgs e)
		{
			timer_result = true;
			StartButton.IsEnabled = false;
			StopButton.IsEnabled = true;

			int timerInterval = 0;
			try
			{
				int.TryParse(InputTimer.Text.ToString(), out timerInterval);
			}
			catch
			{
				timerInterval = 1000;
			}

			//timer = new Timer(timer_Tick, null, 0, timerInterval);
			Device.StartTimer(TimeSpan.FromMilliseconds(timerInterval), () =>
			{
				Device.InvokeOnMainThreadAsync(() =>
				{
					if (timer_result)
						timer_Tick(null);
				});
				return timer_result;
			});
		}
		private void timer_Tick(object state)
		{
			NextFrame();
		}
		private void NextFrame()
		{
			MakeMove();
			RefreshArena();
			PrevFrameButton.IsEnabled = true;
		}
		private void Stop_Click(object sender, EventArgs e)
		{
			StartButton.IsEnabled = true;
			StopButton.IsEnabled = false;

			timer_result = false;
		}
		private void PrevFrame_Click(object sender, EventArgs e)
		{
			if (currentFrame <= 0)
			{
				return;
			}
			currentFrame--;
			if (currentFrame <= 0)
			{
				PrevFrameButton.IsEnabled = false;
			}
			moves.RemoveAt(moves.Count - 1);

			Born.Text = "-";
			Dead.Text = "-";
			RefreshArena();
		}
		private void MakeMove()
		{
			var nextMove = new bool[arenaSizeX, arenaSizeY];

			int born = 0;
			int dead = 0;
			for (int y = 0; y < arenaSizeY; y++)
			{
				for (int x = 0; x < arenaSizeX; x++)
				{
					if (IsAlive(x, y) &&
						(GetLiveNeighborsCount(x, y) == 2 || GetLiveNeighborsCount(x, y) == 3))
					{
						nextMove[x, y] = true;
					}
					else if (!IsAlive(x, y) && GetLiveNeighborsCount(x, y) == 3)
					{
						nextMove[x, y] = true;
					}
					else
					{
						nextMove[x, y] = false;
					}

					if (moves[currentFrame][x, y] == true && nextMove[x, y] == false)
					{
						dead++;
					}
					else if (moves[currentFrame][x, y] == false && nextMove[x, y] == true)
					{
						born++;
					}
				}
			}
			Born.Text = born.ToString();
			Dead.Text = dead.ToString();
			moves.Add(nextMove);
			currentFrame++;
		}
		private void RefreshArena()
		{
			Trace.WriteLine("Refreshing. Frame:" + currentFrame.ToString());
			var current = moves[currentFrame];
			for (int y = 0; y < arenaSizeY; y++)
			{
				for (int x = 0; x < arenaSizeX; x++)
				{
					if (current[x, y])
					{
						arena[x, y].Color = OCCUPIED_FIELD_COLOR;
					}
					else
					{
						arena[x, y].Color = EMPTY_FIELD_COLOR;
					}
				}
			}
			Generation.Text = currentFrame.ToString();
		}
		private void NextFrame_Click(object sender, EventArgs e)
		{
			NextFrame();
		}
		private bool IsAlive(in int x, in int y)
		{
			var current = moves[currentFrame];
			if (x < 0) return false;
			if (y < 0) return false;
			if (x >= arenaSizeX) return false;
			if (y >= arenaSizeY) return false;
			return current[x, y];
		}
		private int GetLiveNeighborsCount(in int x, in int y)
		{
			Point[] neighbors =
		   {
				new Point(x - 1, y - 1),
				new Point(x - 1, y),
				new Point(x - 1, y + 1),
				new Point(x    , y - 1),
				new Point(x    , y + 1),
				new Point(x + 1, y - 1),
				new Point(x + 1, y),
				new Point(x + 1, y + 1)
			};

			int result = 0;
			foreach (var neighbor in neighbors)
			{
				if (IsAlive(Convert.ToInt32(neighbor.X), Convert.ToInt32(neighbor.Y)))
				{
					result++;
				}
			}
			return result;
		}
		private string GetFileName()
		{
			if (string.IsNullOrEmpty(InputFileName.Text))
			{
				return System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "DefaultFile");
				//return "DefaultFile";
			}
			else
				return System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), InputFileName.Text);
				//return InputFileName.Text;
		}
		private void Save_Click(object sender, EventArgs e)
		{
			using (StreamWriter writer = new StreamWriter(GetFileName()))
			{
				writer.WriteLine(arenaSizeX.ToString() + " " + arenaSizeY.ToString());
				for (int y = 0; y < arenaSizeY; y++)
				{
					var row = "";
					for (int x = 0; x < arenaSizeX; x++)
					{
						row += moves[currentFrame][x, y] + " ";
					}
					writer.WriteLine(row);
				}
			}
		}
		private void Load_Click(object sender, EventArgs e)
		{
			string line = "";
			using (StreamReader sr = new StreamReader(GetFileName()))
			{
				line = sr.ReadLine();
				var size = line.Split(" ");

				arenaSizeX = Convert.ToInt32(size[0]);
				arenaSizeY = Convert.ToInt32(size[1]);
				CreateFields();

				for (int y = 0; (line = sr.ReadLine()) != null && y < arenaSizeY; y++)
				{
					var temp = line.Split(" ");
					for (int x = 0; x < arenaSizeX; x++)
					{
						if (temp[x] == "True")
						{
							moves[currentFrame][x, y] = true;
						}
						else if (temp[x] == "False")
						{
							moves[currentFrame][x, y] = false;
						}
					}
				}
			}
			RefreshArena();
			CreateArena.IsEnabled = false;
			NextFrameButton.IsEnabled = true;
			StartButton.IsEnabled = true;
			LoadButton.IsEnabled = false;
			SaveButton.IsEnabled = true;
		}
	}
}