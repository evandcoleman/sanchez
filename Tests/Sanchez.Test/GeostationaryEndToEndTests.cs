﻿using System.IO;
using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;
using Sanchez.Processing.Models;
using SixLabors.ImageSharp;

namespace Sanchez.Test
{
    [TestFixture(TestOf = typeof(Bootstrapper))]
    public class GeostationaryEndToEndTests : EndToEndTestTests
    {
        [Test]
        public async Task Single()
        {
            var rootDirectory = await CreateSampleImagesAsync(State);
            var outputDirectory = State.CreateTempDirectory();
            var outputFile = Path.Combine(outputDirectory, "out.jpg");

            var returnCode = await Bootstrapper.Main(
                "-s", Path.Combine(rootDirectory, "GOES16_FD_CH13_20200830T035020Z.jpg"),
                "-o", outputFile,
                "-O", "1.0",
                "-q");

            VerifySuccessfulExecution(returnCode);

            File.Exists(outputFile).Should().BeTrue("output file should have been created");
            var outputImage = await Image.LoadAsync(outputFile);
            outputImage.Width.Should().Be(Constants.Satellite.ImageSize.FourKm);
            outputImage.Height.Should().Be(Constants.Satellite.ImageSize.FourKm);
        }

        [Test]
        public async Task SingleWithOverlay()
        {
            var rootDirectory = await CreateSampleImagesAsync(State);
            await CreateImage(Path.Combine(rootDirectory, "Overlay.jpg"));

            var outputDirectory = State.CreateTempDirectory();
            var outputFile = Path.Combine(outputDirectory, "out.jpg");

            var returnCode = await Bootstrapper.Main(
                "-s", Path.Combine(rootDirectory, "GOES16_FD_CH13_20200830T035020Z.jpg"),
                "-o", outputFile,
                "-c", "0.2-1.0",
                "-q");

            VerifySuccessfulExecution(returnCode);

            File.Exists(outputFile).Should().BeTrue("output file should have been created");
            var outputImage = await Image.LoadAsync(outputFile);
            outputImage.Width.Should().Be(Constants.Satellite.ImageSize.FourKm);
            outputImage.Height.Should().Be(Constants.Satellite.ImageSize.FourKm);
        }

        [Test]
        public async Task Multiple()
        {
            var rootDirectory = await CreateSampleImagesAsync(State);
            var outputDirectory = State.CreateTempDirectory();

            var returnCode = await Bootstrapper.Main(
                "-s", rootDirectory,
                "-o", outputDirectory,
                "-q");

            VerifySuccessfulExecution(returnCode);

            Directory.Exists(outputDirectory).Should().BeTrue("output directory should have been created");
            Directory.GetFiles(outputDirectory).Should().HaveCount(8);

            foreach (var outputFile in Directory.GetFiles(outputDirectory))
            {
                var outputImage = await Image.LoadAsync(outputFile);
                outputImage.Width.Should().Be(Constants.Satellite.ImageSize.FourKm);
                outputImage.Height.Should().Be(Constants.Satellite.ImageSize.FourKm);
            }
        }

        [Test]
        public async Task Reprojected()
        {
            var rootDirectory = await CreateSampleImagesAsync(State);
            var outputDirectory = State.CreateTempDirectory();
            var outputFile = Path.Combine(outputDirectory, "out.jpg");

            var returnCode = await Bootstrapper.Main(
                "-s", rootDirectory,
                "-o", outputFile,
                "-l", "174",
                "-T 2020-08-30T03:50:20",
                "-qv");

            VerifySuccessfulExecution(returnCode);

            File.Exists(outputFile).Should().BeTrue("output file should have been created");
            var outputImage = await Image.LoadAsync(outputFile);
            outputImage.Width.Should().Be(Constants.Satellite.ImageSize.FourKm);
            outputImage.Height.Should().Be(Constants.Satellite.ImageSize.FourKm);
        }

        [Test]
        public async Task ReprojectedOutputDirectory()
        {
            var rootDirectory = await CreateSampleImagesAsync(State);
            var outputDirectory = State.CreateTempDirectory();

            var returnCode = await Bootstrapper.Main(
                "-s", rootDirectory,
                "-o", outputDirectory,
                "-l", "174",
                "-T 2020-08-30T03:50:20",
                "-qv");

            VerifySuccessfulExecution(returnCode);

            Directory.Exists(outputDirectory).Should().BeTrue("output directory should have been created");
            Directory.GetFiles(outputDirectory).Should().HaveCount(1);

            foreach (var outputFile in Directory.GetFiles(outputDirectory))
            {
                var outputImage = await Image.LoadAsync(outputFile);
                outputImage.Width.Should().Be(Constants.Satellite.ImageSize.FourKm);
                outputImage.Height.Should().Be(Constants.Satellite.ImageSize.FourKm);
            }
        }

        [Test]
        public async Task ReprojectedMinSatellites()
        {
            var rootDirectory = await CreateSampleImagesAsync(State);
            var outputDirectory = State.CreateTempDirectory();
            var outputFile = Path.Combine(outputDirectory, "out.jpg");

            var returnCode = await Bootstrapper.Main(
                "-s", rootDirectory,
                "-o", outputFile,
                "-l", "174",
                "-m", "3",
                "-T 2020-08-30T03:50:20",
                "-q");

            VerifySuccessfulExecution(returnCode);

            File.Exists(outputFile).Should().BeTrue("output file should have been created");
            var outputImage = await Image.LoadAsync(outputFile);
            outputImage.Width.Should().Be(Constants.Satellite.ImageSize.FourKm);
            outputImage.Height.Should().Be(Constants.Satellite.ImageSize.FourKm);
        }

        [Test]
        public async Task ReprojectedTooFewSatellites()
        {
            var rootDirectory = await CreateSampleImagesAsync(State);
            var outputDirectory = State.CreateTempDirectory();
            var outputFile = Path.Combine(outputDirectory, "out.jpg");

            var returnCode = await Bootstrapper.Main(
                "-s", rootDirectory,
                "-o", outputFile,
                "-l", "174",
                "-m", "10",
                "-T 2020-08-30T03:50:20",
                "-q");

            VerifySuccessfulExecution(returnCode);
            File.Exists(outputFile).Should().BeFalse("fewer than 10 satellites are present");
        }

        [Test]
        public async Task TimestampReprojected()
        {
            var rootDirectory = await CreateSampleImagesAsync(State);
            var outputDirectory = State.CreateTempDirectory();

            await Bootstrapper.Main(
                "-s", rootDirectory,
                "-o", outputDirectory,
                "-l", "174",
                "-I", "60",
                "-c", "0.5-1",
                "-q");

            Directory.Exists(outputDirectory).Should().BeTrue("output directory should have been created");
            Directory.GetFiles(outputDirectory).Should().HaveCount(2);

            foreach (var outputFile in Directory.GetFiles(outputDirectory))
            {
                var outputImage = await Image.LoadAsync(outputFile);
                outputImage.Width.Should().Be(Constants.Satellite.ImageSize.FourKm);
                outputImage.Height.Should().Be(Constants.Satellite.ImageSize.FourKm);
            }
        }

        [Test]
        public async Task TimestampReprojectedRotation()
        {
            var rootDirectory = await CreateSampleImagesAsync(State);
            var outputDirectory = State.CreateTempDirectory();

            await Bootstrapper.Main(
                "-s", rootDirectory,
                "-o", outputDirectory,
                "-l", "174",
                "-E", "-20",
                "-I", "60",
                "--inverse",
                "-e", "2020-08-30T05:50:20",
                "-c", "0.0-0.1",
                "-q");

            Directory.Exists(outputDirectory).Should().BeTrue("output directory should have been created");
            Directory.GetFiles(outputDirectory).Should().HaveCount(2);

            foreach (var outputFile in Directory.GetFiles(outputDirectory))
            {
                var outputImage = await Image.LoadAsync(outputFile);
                outputImage.Width.Should().Be(Constants.Satellite.ImageSize.FourKm);
                outputImage.Height.Should().Be(Constants.Satellite.ImageSize.FourKm);
            }
        }
    }
}