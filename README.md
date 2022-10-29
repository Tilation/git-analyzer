# Git Analyzer

This software was made to have a more visual way of doing GIT things.

🆗 What you can do with this software:
- View the historic list of files in a repository, showing path, file size, and git hash id. This is useful for cleaning repositories alongside [bfg repo cleaner](https://rtyley.github.io/bfg-repo-cleaner/), copy and paste the file hash ids into a file and then purge using that file.
- Extend this software, yes you can write your own plugins and commands using C#, there is a specific zip file in the releases where you have a solution already configured for plugin development.

📅 Planned:
- More actions.
- Better documentation.
- Better plugin framework.

## ⚒️ Creating plugins 🛠️
Once you run the aplication, a folder called `/modules` will be created, you need to put the dll plugins there.

To create a plugin you require a few things:
- 🧰 Visual Studio
- 🧠 C# Knowledge & intermediate OOP

Steps:
1. ⬇️ Download and extract the latest release of the plugin creation solution from [here](https://github.com/Tilation/git-analyzer/releases/latest)
2. 📁 Open the file `Plugins.sln` with visual studio.
3. 🆕 Create the proyect for your plugin `File > New > Proyect` and select `Class Library (.Net Framework)` In the next window, choose a name for your plugin and in the `Solution` option, choose `Add to Solution` and choose `.Net Framework 4.7.2`
4. 🔗 Inside your proyect in the `Solution Explorer`, right click `References` and add a proyect reference to `GitAnalyzer.Modules`.
5. ✍️ Now you can start writing a plugin, note that all plugins must implement `BaseModule` for the software to recognize it as a plugin, so have at least one class that implement it.
6. 🕵️ Check out the classes in `GitAnalyzer.Modules` to see what you can do, specifically the protected ones on `GitAnalyzer.Modules.BaseModule`
