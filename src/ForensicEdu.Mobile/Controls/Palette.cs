namespace ForensicEdu.Mobile.Controls;
public static class Palette
{
 public static readonly Color Accent=Color.FromArgb("#46EFCF");public static readonly Color Deep=Color.FromArgb("#00382E");
 public static void Apply(bool dark){Application.Current!.UserAppTheme=dark?AppTheme.Dark:AppTheme.Light;var r=Application.Current.Resources;r["Lowest"]=Color.FromArgb(dark?"#090E1C":"#EDF2F5");r["Outline"]=Color.FromArgb(dark?"#85948F":"#627580");r["Canvas"]=Color.FromArgb(dark?"#0E1321":"#F5F8FA");r["Surface"]=Color.FromArgb(dark?"#161B2A":"#FFFFFF");r["Raised"]=Color.FromArgb(dark?"#252A39":"#E9F0F2");r["Ink"]=Color.FromArgb(dark?"#DEE2F6":"#142536");r["Muted"]=Color.FromArgb(dark?"#BACAC4":"#647580");r["Line"]=Color.FromArgb(dark?"#303C42":"#D9E2E7");Preferences.Default.Set("dark-theme",dark);}
}
