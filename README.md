# StarGraph

**StarGraph uses Azure Functions to automatically generate graphs of GitHub stars over time** and save them as static images that can be displayed inside GitHub readme pages. 

StarGraph uses the GitHub API to retrieve stargazer timestamps, store records in Azure blob storage (minimizing the number of API requests), create a plot of historical stars using ScottPlot, then save the result in an Azure blob storage container that has a web-accessible URL.

## Live Demo ([ScottPlot](https://github.com/ScottPlot/ScottPlot))

<p align="center">
  <img src="https://stargraph.z20.web.core.windows.net/scottplot-stars.png">
</p>
