using FluentAssertions;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;
using System.Globalization;

namespace BlazeDemoTests;

[TestFixture]
public class BlazeDemoTests
{
    private IWebDriver driver;
    private const string BaseURL = "https://blazedemo.com";

    [SetUp]
    public void SetupTest()
    {
        driver = new ChromeDriver();
        driver.Manage().Window.Maximize();
    }

    [TearDown]
    public void TeardownTest()
    {
        driver.Quit();
        driver.Dispose();
    }

    [TestCase(3)]
    public void BlazeDemo_MexicoCityToDublin_ShouldHaveAtLeastNumberOfFlights(int numberOfFlights)
    {
        // Arrange
        driver.Navigate().GoToUrl(BaseURL);
        var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(5));


        var fromPortDropdown = wait.Until(
            ExpectedConditions.ElementIsVisible(By.Name("fromPort")));

        var toPortDropdown = wait.Until(
            ExpectedConditions.ElementIsVisible(By.Name("toPort")));

        new SelectElement(fromPortDropdown).SelectByText("Mexico City");

        new SelectElement(toPortDropdown).SelectByText("Dublin");

        // Act
        var findFlightsButton = wait.Until(ExpectedConditions.ElementToBeClickable(By.XPath("//input[@value='Find Flights']")));
        findFlightsButton.Click();


        // Assert
        wait.Until(ExpectedConditions.ElementIsVisible(By.XPath("//table")));

        var flights = driver.FindElements(By.XPath("//table/tbody/tr"));

        flights.Count.Should().BeGreaterThanOrEqualTo(numberOfFlights);
    }

    [TestCase(20)]
    public void BlazeDemo_MexicoCityToDublin_ShouldTakeScreenshot_WhenCheapFlightExists(double maximumPrice)
    {
        driver.Navigate().GoToUrl(BaseURL);

        var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(5));

        new SelectElement(wait.Until(
            ExpectedConditions.ElementIsVisible(By.Name("fromPort"))))
            .SelectByText("Mexico City");

        new SelectElement(wait.Until(
            ExpectedConditions.ElementIsVisible(By.Name("toPort"))))
            .SelectByText("Dublin");

        // Act
        var findFlightsButton = wait.Until(
            ExpectedConditions.ElementToBeClickable(By.XPath("//input[@value='Find Flights']")));
        findFlightsButton.Click();

        // Assert
        wait.Until(ExpectedConditions.ElementIsVisible(By.XPath("//table")));
        var flights = driver.FindElements(By.XPath("//table/tbody/tr"));

        IWebElement? cheapFlightRow = null;
        string? cheapFlightDetails = null;

        foreach (var row in flights)
        {
            var cells = row.FindElements(By.XPath("./td"));
            var priceText = cells.Last().Text
                .Replace("$", "")
                .Trim();

            var price = double.Parse(priceText, CultureInfo.InvariantCulture);

            if (price < maximumPrice)
            {
                cheapFlightRow = row;
                cheapFlightDetails = string.Join(" | ", cells.Select(cell => cell.Text));

                TestContext.WriteLine("Cheap flight found:");
                TestContext.WriteLine(cheapFlightDetails);
                break;
            }
        }

        cheapFlightRow.Should().NotBeNull(
            $"There should be at least one flight to Dublin cheaper than {maximumPrice}.");

        var chooseThisFlightButton = cheapFlightRow!.FindElement(
            By.XPath(".//input[@value='Choose This Flight']"));
        chooseThisFlightButton.Click();


        wait.Until(ExpectedConditions.ElementIsVisible(By.XPath("//h2")));
        var screenshot = ((ITakesScreenshot)driver).GetScreenshot();
        var desktopPath = Environment.GetFolderPath(
            Environment.SpecialFolder.DesktopDirectory);
        var screenshotPath = Path.Combine(
            desktopPath,
            "blazedemo-cheap-dublin-flight.png");
        screenshot.SaveAsFile(screenshotPath);
        
    }
}