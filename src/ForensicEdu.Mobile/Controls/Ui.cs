using Microsoft.Maui.Controls.Shapes;
namespace ForensicEdu.Mobile.Controls;

public static class Ui
{
 public static Label Text(string value,double size=14,bool bold=false,string color="Ink")
 {
  var label=new Label{Text=value,FontSize=size,FontFamily=bold?"JakartaBold":"Jakarta",LineBreakMode=LineBreakMode.WordWrap,LineHeight=1.15};
  label.SetDynamicResource(Label.TextColorProperty,color);return label;
 }
 public static Label Eyebrow(string value)=>new(){Text=value.ToUpperInvariant(),FontFamily="JakartaBold",FontSize=9,CharacterSpacing=.8,TextColor=Palette.Accent};
 public static VerticalStackLayout Stack(params View[] views){var stack=new VerticalStackLayout{Spacing=10};foreach(var view in views)stack.Add(view);return stack;}
 public static Border Card(View child,bool accent=false)
 {
  var border=new Border{Content=child,Padding=14,StrokeThickness=0,StrokeShape=new RoundRectangle{CornerRadius=8}};
  border.SetDynamicResource(VisualElement.BackgroundColorProperty,"Surface");
  if(accent){border.StrokeThickness=1;border.Stroke=Color.FromArgb("#28544F");}return border;
 }
 public static Button Action(string text,Func<Task> action,bool secondary=false)
 {
  var button=new Button{Text=text,FontFamily="JakartaBold",FontSize=12,CornerRadius=7,MinimumHeightRequest=44,BackgroundColor=secondary?Colors.Transparent:Palette.Accent,TextColor=secondary?Palette.Accent:Palette.Deep,Padding=new Thickness(12,10)};
  if(secondary)button.SetDynamicResource(VisualElement.BackgroundColorProperty,"Raised");
  button.Clicked+=async(_,_)=>{if(!button.IsEnabled)return;button.IsEnabled=false;try{await action();}finally{if(button.Window!=null&&button.Handler!=null)button.IsEnabled=true;}};return button;
 }
 public static Border Field(string label,View field)
 {
  var content=Stack(Text(label,11,true),field);content.Spacing=7;
  return new Border{Content=content,Padding=0,StrokeThickness=0,BackgroundColor=Colors.Transparent};
 }
 public static Entry Entry(string placeholder,bool password=false,Keyboard? keyboard=null)
 {
  var entry=new Entry{Placeholder=placeholder,IsPassword=password,Keyboard=keyboard??Keyboard.Default,FontSize=12,FontFamily="Jakarta",HeightRequest=46,BackgroundColor=Colors.Transparent};
  entry.SetDynamicResource(Microsoft.Maui.Controls.Entry.TextColorProperty,"Ink");entry.SetDynamicResource(Microsoft.Maui.Controls.Entry.PlaceholderColorProperty,"Outline");return entry;
 }
 public static Border Input(Entry entry,string icon,View? trailing=null)
 {
  var grid=new Grid{ColumnDefinitions={new ColumnDefinition(28),new ColumnDefinition(GridLength.Star),new ColumnDefinition(GridLength.Auto)},ColumnSpacing=4};
  grid.Add(new Image{Source=icon,WidthRequest=17,HeightRequest=17,VerticalOptions=LayoutOptions.Center});grid.Add(entry,1);if(trailing!=null)grid.Add(trailing,2);
  var border=new Border{Content=grid,Padding=new Thickness(10,0),StrokeThickness=0,StrokeShape=new RoundRectangle{CornerRadius=5}};
  border.SetDynamicResource(VisualElement.BackgroundColorProperty,"Lowest");return border;
 }
 public static Grid Row(View left,View right){var grid=new Grid{ColumnDefinitions={new ColumnDefinition(GridLength.Star),new ColumnDefinition(GridLength.Auto)},ColumnSpacing=10};grid.Add(left);grid.Add(right,1);return grid;}
 public static ProgressBar Progress(double value)=>new(){Progress=Math.Clamp(value,0,1),ProgressColor=Palette.Accent,BackgroundColor=Color.FromArgb("#303C42"),HeightRequest=5};
 public static View Brand()
 {
  var name=Text("FORENSIQ",14,true);var subtitle=Text("LAB ACADÉMICO",8,true,"Muted");var text=Stack(name,subtitle);text.Spacing=0;
  var left=new HorizontalStackLayout{Spacing=9,Children={new Image{Source="logo.png",HeightRequest=40,WidthRequest=40},text}};
  var badge=Card(new Image{Source="profile.png",HeightRequest=15,WidthRequest=15});badge.Padding=8;badge.BackgroundColor=Palette.Accent;
  return Row(left,badge);
 }
}
