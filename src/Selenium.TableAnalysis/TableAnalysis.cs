using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace SeleniumCsharp
{
    public class Tests

    {

        IWebDriver driver;
        [OneTimeSetUp]
        public void Setup()
        {
            string path = Directory.GetParent(Environment.CurrentDirectory).Parent.Parent.FullName;
            driver = new ChromeDriver(path + @"\Drivers\");
        }

        [Test]
        public void BuildGenericPathForDivA()
        {
            driver.Navigate().GoToUrl("http://127.0.0.1:8080/div-a");
            var dateElement = driver.FindElement(By.XPath("/html/body/div/div/div[1]/div/div[1]/span"));
            var linkElement = driver.FindElement(By.XPath("/html/body/div/div/div[1]/div/div[2]/a/span"));

            var xpathDate = GetXpath(dateElement);
            var xpathLink = GetXpath(linkElement);

            var container = FindRoot(dateElement, linkElement);
            var xpath = GetXpath(container);

            var xpathSuffixDate = xpathDate.Remove(0, xpath.Length);
            var xpathSuffixLink = xpathLink.Remove(0, xpath.Length);

            xpath = GenerateDynamicRootPath(xpath, xpathSuffixLink, 0);

            driver.Quit();
            Assert.IsTrue(true);
        }

        [Test]
        public void BuildGenericPathForDivB()
        {
            driver.Navigate().GoToUrl("http://127.0.0.1:8080/div-b");
            var dateElement = driver.FindElement(By.XPath("/html/body/div/div/div[1]/div/div[1]/span"));
            var linkElement = driver.FindElement(By.XPath("/html/body/div/div/div[1]/div/div[2]/a/span"));

            var xpathDate = GetXpath(dateElement);
            var xpathLink = GetXpath(linkElement);

            var container = FindRoot(dateElement, linkElement);

            var containerParent = GetParent(container);

            var children = containerParent.FindElements(By.XPath(".//*"));
            foreach(var child in children)
            {
                var childPath = GetXpath(child);
            }

            var xpath = GetXpath(container);

            var xpathSuffixDate = xpathDate.Remove(0, xpath.Length);
            var xpathSuffixLink = xpathLink.Remove(0, xpath.Length);

            xpath = GenerateDynamicRootPath(xpath, xpathSuffixLink, 0);

            driver.Quit();
            Assert.IsTrue(true);
        }

        [Test]
        public void BuildGenericPathForTableA()
        {
            driver.Navigate().GoToUrl("http://127.0.0.1:8080/table-a");
            var dateElement = driver.FindElement(By.XPath("/html/body/table/tbody/tr[2]/td[1]"));
            var linkElement = driver.FindElement(By.XPath("/html/body/table/tbody/tr[2]/td[3]/a/span"));

            var xpathDate = GetXpath(dateElement);
            var xpathLink = GetXpath(linkElement);

            var container = FindRoot(dateElement, linkElement);

            var xpath = GetXpath(container);

            var xpathSuffixDate = xpathDate.Remove(0, xpath.Length);
            var xpathSuffixLink = xpathLink.Remove(0, xpath.Length);

            xpath = GenerateDynamicRootPath(xpath, xpathSuffixLink, 0);

            driver.Quit();
            Assert.IsTrue(true);
        }


        [Test]
        public void BuildGenericPathForTableB()
        {
            driver.Navigate().GoToUrl("http://127.0.0.1:8080/table-b");
            var dateElement = driver.FindElement(By.XPath("/html/body/table/tbody/tr[2]/td[1]"));
            var linkElement = driver.FindElement(By.XPath("/html/body/table/tbody/tr[2]/td[3]/a/span"));

            var xpathDate = GetXpath(dateElement);
            var xpathLink = GetXpath(linkElement);

            var container = FindRoot(dateElement, linkElement);
            var xpath = GetXpath(container);

            var xpathSuffixDate = xpathDate.Remove(0, xpath.Length);
            var xpathSuffixLink = xpathLink.Remove(0, xpath.Length);

            xpath = GenerateDynamicRootPath(xpath, xpathSuffixLink, 0);

            driver.Quit();
            Assert.IsTrue(true);
        }

        private IWebElement FindRoot(IWebElement elementA, IWebElement elementB)
        {
            var parentA = GetParent(elementA);
            var parentB = GetParent(elementB);

            var aXpath = GetXpath(parentA);
            var bXpath = GetXpath(parentB);

            var isAHasB = bXpath.StartsWith(aXpath);
            var isBHasA = aXpath.StartsWith(bXpath);

            if (aXpath == bXpath
                    || isAHasB)
            {
                return parentA;
            }
            else if (isBHasA)
            {
                return parentB;
            }
            else
            {
                return FindRoot(parentA, parentB);
            }
        }

        private IWebElement GetParent(IWebElement element)
        {
            try
            {
                return element.FindElement(By.XPath("./parent::*"));
            }
            catch (NoSuchElementException ex)
            {
                return element;
            }
        }

        public static String GetXpath(IWebElement element)
        {
            int n = element.FindElements(By.XPath("./ancestor::*")).Count;
            String path = "";
            IWebElement current = element;
            for (int i = n; i > 0; i--)
            {
                String tag = current.TagName;
                int lvl = current.FindElements(By.XPath("./preceding-sibling::" + tag)).Count + 1;
                path = String.Format($"/{tag}[{lvl}]{path}");
                current = current.FindElement(By.XPath("./parent::*"));
            }
            return "/" + current.TagName + path;
        }

        public string GenerateDynamicRootPath(string path, string verificationTargetPath, int lastIndex)
        {
            try
            {
                var regex = @"(?<=\[)(.*?)(?=\])";
                var items = path.Split('/');
                var target = items[items.Length - (lastIndex + 1)];
                items[items.Length - 1 - lastIndex] = Regex.Replace(target, regex, "2");
                var _path = $"{string.Join("/", items)}{verificationTargetPath}";
                if (driver.FindElement(By.XPath(_path)).Displayed)
                {
                    target = items[items.Length - (lastIndex + 1)];
                    items[items.Length - 1 - lastIndex] = Regex.Replace(target, regex, "{RowNo}");
                    return string.Join("/", items);
                }
                return GenerateDynamicRootPath(path, verificationTargetPath, ++lastIndex);
            }
            catch (NoSuchElementException)
            {
                return GenerateDynamicRootPath(path, verificationTargetPath, ++lastIndex);
            }
        }
    }

}
