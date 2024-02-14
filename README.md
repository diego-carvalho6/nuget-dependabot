# nuget-dependabot

Its similar to dependabot core, but actually only works with .net dependences in a .csproj file and using the <PackacgeReference/> tag.
Works Grouping all possible updates in one unique pull request, and add the current date operation in a chanlog.md file.
The 'with' action options include:

<ul>
  <li>'nuget-source' to allow specify a private source</li>
  <li>'allowed-sources' to allow define exclusive update, or prefix package name, splited by '|'</li>
  <li>'workdir' to specify the current dir to start searching csproj files, default is '/'</li>
  <li>'log-file-name' default log file name</li>
</ul>

