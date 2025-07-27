[![Contributors][contributors-shield]][contributors-url]
[![Forks][forks-shield]][forks-url]
[![Stargazers][stars-shield]][stars-url]
[![Issues][issues-shield]][issues-url]
[![project_license][license-shield]][license-url]
[![LinkedIn][linkedin-shield]][linkedin-url]



<!-- PROJECT LOGO -->
<br />
<div align="center">
  <a href="https://github.com/viskvortsov/Light.Speed.DbClient">
    <img src="icons/main.png" alt="Logo" width="80" height="80">
  </a>

<h3 align="center">LightSpeed.DbClient</h3>

  <p align="center">
    A C# database client optimized for speed and code efficiency 
    <br />
    <a href="https://github.com/github_username/repo_name/issues/new?labels=bug&template=bug-report---.md">Report Bug</a>
    &middot;
    <a href="https://github.com/github_username/repo_name/issues/new?labels=enhancement&template=feature-request---.md">Request Feature</a>
  </p>
</div>

<!-- TABLE OF CONTENTS -->
<details>
  <summary>Table of Contents</summary>
  <ol>
    <li>
      <a href="#about-the-project">About The Project</a>
    </li>
    <li>
      <a href="#getting-started">Getting Started</a>
      <ul>
        <li><a href="#prerequisites">Prerequisites</a></li>
        <li><a href="#installation">Installation</a></li>
      </ul>
    </li>
    <li><a href="#usage">Usage</a></li>
    <li><a href="#roadmap">Roadmap</a></li>
    <li><a href="#contributing">Contributing</a></li>
    <li><a href="#license">License</a></li>
    <li><a href="#contact">Contact</a></li>
    <li><a href="#acknowledgments">Acknowledgments</a></li>
  </ol>
</details>



<!-- ABOUT THE PROJECT -->
## About The Project

A C# database client optimized for speed and code efficiency

<p align="right">(<a href="#readme-top">back to top</a>)</p>


<!-- GETTING STARTED -->
## Getting Started

This is an example of how you may give instructions on setting up your project locally.
To get a local copy up and running follow these simple example steps.

### Prerequisites

# TODO

<p align="right">(<a href="#readme-top">back to top</a>)</p>

### Installation

# TODO

<p align="right">(<a href="#readme-top">back to top</a>)</p>



<!-- USAGE EXAMPLES -->
## Usage

```
IDatabase db = new PostgresqlDatabase("localhost",5432,"backend", "backend", "mysecretpassword");
IConnection connection = await db.OpenConnectionAsync();
ITransaction transaction = await connection.BeginTransactionAsync();
IManager<Product> productManager = new PostgresqlManager<Product>(connection, transaction);    

try {

  Product product = productManager.CreateObject();
  product.Id = Guid.NewGuid();
  
  AttributeRow row = new AttributeRow
  {
      Id = Guid.NewGuid(),
      Attribute = attribute.Id,
      AttributeName = attributeName,
      Value = value,
  };
  product.Attributes = new DatabaseObjectTable<AttributeRow>();
  product.Attributes.Add(row);
  
  var result = await productManager.SaveAsync(product);
  await transaction.CommitAsync();
  
} catch (Exception e)
{
    await transaction.RollbackAsync();
}
```

<p align="right">(<a href="#readme-top">back to top</a>)</p>



<!-- ROADMAP -->
## Roadmap

- Postgresql implementation refactoring

See the [open issues](https://github.com/github_username/Light.Speed.DbClient/issues) for a full list of proposed features (and known issues).

<p align="right">(<a href="#readme-top">back to top</a>)</p>



<!-- CONTRIBUTING -->
## Contributing

Contributions are what make the open source community such an amazing place to learn, inspire, and create. Any contributions you make are **greatly appreciated**.

If you have a suggestion that would make this better, please fork the repo and create a pull request. You can also simply open an issue with the tag "enhancement".
Don't forget to give the project a star! Thanks again!

1. Fork the Project
2. Create your Feature Branch (`git checkout -b feature/AmazingFeature`)
3. Commit your Changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to the Branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

<p align="right">(<a href="#readme-top">back to top</a>)</p>

### Top contributors:

<a href="https://github.com/viskvortsov/Light.Speed.DbClient/graphs/contributors">
  <img src="https://contrib.rocks/image?repo=viskvortsov/Light.Speed.DbClient" alt="contrib.rocks image" />
</a>



<!-- LICENSE -->
## License

Distributed under the project_license. See `LICENSE.txt` for more information.

<p align="right">(<a href="#readme-top">back to top</a>)</p>



<!-- CONTACT -->
## Contact

Your Name - [@viskvortsov](https://www.linkedin.com/in/viskvortsov) - viskvortsovkrsk@gmail.com

<p align="right">(<a href="#readme-top">back to top</a>)</p>



<!-- ACKNOWLEDGMENTS -->
## Acknowledgments

TODO

<p align="right">(<a href="#readme-top">back to top</a>)</p>



<!-- MARKDOWN LINKS & IMAGES -->
<!-- https://www.markdownguide.org/basic-syntax/#reference-style-links -->
[contributors-shield]: https://img.shields.io/github/contributors/viskvortsov/Light.Speed.DbClient.svg?style=for-the-badge
[contributors-url]: https://github.com/viskvortsov/Light.Speed.DbClient/graphs/contributors
[forks-shield]: https://img.shields.io/github/forks/viskvortsov/Light.Speed.DbClient.svg?style=for-the-badge
[forks-url]: https://github.com/viskvortsov/repo_name/network/members
[stars-shield]: https://img.shields.io/github/stars/viskvortsov/Light.Speed.DbClient.svg?style=for-the-badge
[stars-url]: https://github.com/viskvortsov/repo_name/stargazers
[issues-shield]: https://img.shields.io/github/issues/viskvortsov/Light.Speed.DbClient.svg?style=for-the-badge
[issues-url]: https://github.com/viskvortsov/repo_name/issues
[license-shield]: https://img.shields.io/github/license/viskvortsov/Light.Speed.DbClient.svg?style=for-the-badge
[license-url]: https://github.com/viskvortsov/repo_name/blob/master/LICENSE.txt
[linkedin-shield]: https://img.shields.io/badge/-LinkedIn-black.svg?style=for-the-badge&logo=linkedin&colorB=555
[linkedin-url]: https://www.linkedin.com/in/viskvortsov