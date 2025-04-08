<div align="center">
    <img src="https://raw.githubusercontent.com/RiptideEngine/Lunacub/refs/heads/main/icon.png" alt="icon" width="256" height="256"/>
</div>

<h1 align="center">Lunacub</h1>

Lunacub is an API designed to simplify the process of building and importing external
resources during application runtime.

Always built with latest .NET version (currently .NET 9.0).

## License

Lunacub is licensed under the GNU General Public License Version 3, see [here](https://www.gnu.org/licenses/gpl-3.0.en.html#license-text) for more details.

## Documentation

- There is currently no plan to make a documentation website, but the [Playground](/Lunacub.Playground) is an application that uses the library to build and import the game resources.
## Manual building

If you prefer to use the library in your own .NET environment, you can compile the API yourself (Note: There is no guarantee that it will build successfully, if that's the case, *question yourself why you want to stay at obsolete versions*).

To clone Lunacub locally, run one of these 2 following git commands:

```bash
git clone https://github.com/RiptideEngine/Lunacub.git
```
or
```bash
git clone git@github.com:RiptideEngine/Lunacub.git
```

If you are on Windows, you can optionally enable long file paths in git (run as Administrator):

```bash
git config --system core.longpaths true
```