using ForensicEdu.Mobile.Pages;
namespace ForensicEdu.Mobile;
public sealed class AppShell:Shell
{
 public AppShell(IServiceProvider services){FlyoutBehavior=FlyoutBehavior.Disabled;SetNavBarIsVisible(this,false);SetDynamicResource(TabBarBackgroundColorProperty,"Lowest");SetDynamicResource(BackgroundColorProperty,"Canvas");SetDynamicResource(Shell.BackgroundColorProperty,"Lowest");SetDynamicResource(Shell.TitleColorProperty,"Ink");SetTabBarForegroundColor(this,Controls.Palette.Accent);SetTabBarTitleColor(this,Controls.Palette.Accent);SetTabBarUnselectedColor(this,Color.FromArgb("#85948F"));
  var bar=new TabBar();
  void Tab(string title,string route,string icon,Func<Page> factory){var content=new ShellContent{Title=title,Route=route,ContentTemplate=new DataTemplate(factory)};var tab=new Tab{Title=title,Icon=icon,Route=route+"-tab"};tab.Items.Add(content);bar.Items.Add(tab);}
  Tab("Catálogo","catalogo","catalog.png",()=>new CatalogPage(services));Tab("Mis cursos","cursos","courses.png",()=>new MyCoursesPage(services));Tab("En vivo","eventos","events.png",()=>new EventsPage(services));Tab("Diplomas","diplomas","certificate.png",()=>new CertificatesPage(services));Tab("Mi cuenta","cuenta","profile.png",()=>new ProfilePage(services));Items.Add(bar);
 }
}
