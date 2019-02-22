/* 
 * Login Server API
 *
 * No description provided (generated by Swagger Codegen https://github.com/swagger-api/swagger-codegen)
 *
 * OpenAPI spec version: v1
 * 
 * Generated by: https://github.com/swagger-api/swagger-codegen.git
 */

using System;
using System.IO;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using RestSharp;
using NUnit.Framework;

using IO.Swagger.Client;
using IO.Swagger.Api;
using IO.Swagger.Model;

namespace IO.Swagger.Test
{
    /// <summary>
    ///  Class for testing CharacterApi
    /// </summary>
    /// <remarks>
    /// This file is automatically generated by Swagger Codegen.
    /// Please update the test case below to test the API endpoint.
    /// </remarks>
    [TestFixture]
    public class CharacterApiTests
    {
        private CharacterApi instance;

        /// <summary>
        /// Setup before each unit test
        /// </summary>
        [SetUp]
        public void Init()
        {
            instance = new CharacterApi();
        }

        /// <summary>
        /// Clean up after each unit test
        /// </summary>
        [TearDown]
        public void Cleanup()
        {

        }

        /// <summary>
        /// Test an instance of CharacterApi
        /// </summary>
        [Test]
        public void InstanceTest()
        {
            // TODO uncomment below to test 'IsInstanceOfType' CharacterApi
            //Assert.IsInstanceOfType(typeof(CharacterApi), instance, "instance is a CharacterApi");
        }

        
        /// <summary>
        /// Test CreateCharacter
        /// </summary>
        [Test]
        public void CreateCharacterTest()
        {
            // TODO uncomment below to test the method and replace null with proper value
            //string name = null;
            //var response = instance.CreateCharacter(name);
            //Assert.IsInstanceOf<CharacterInformation> (response, "response is CharacterInformation");
        }
        
        /// <summary>
        /// Test GetCharacters
        /// </summary>
        [Test]
        public void GetCharactersTest()
        {
            // TODO uncomment below to test the method and replace null with proper value
            //var response = instance.GetCharacters();
            //Assert.IsInstanceOf<List<CharacterInformation>> (response, "response is List<CharacterInformation>");
        }
        
        /// <summary>
        /// Test SelectCharacter
        /// </summary>
        [Test]
        public void SelectCharacterTest()
        {
            // TODO uncomment below to test the method and replace null with proper value
            //string name = null;
            //var response = instance.SelectCharacter(name);
            //Assert.IsInstanceOf<string> (response, "response is string");
        }
        
    }

}
