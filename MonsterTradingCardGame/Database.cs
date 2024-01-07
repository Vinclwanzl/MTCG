using Newtonsoft.Json;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace MonsterTradingCardGame
{
    
    class Database
    {
        public static readonly string _sqlScript = @"
        CREATE TABLE IF NOT EXISTS player 
        (
            username VARCHAR(50) NOT NULL,
            password VARCHAR(50) NOT NULL,
            coinpurse INT NOT NULL,
            bio VARCHAR(250),
            image VARCHAR(10)
        );

        CREATE TABLE IF NOT EXISTS scoreboard 
        (
            username VARCHAR(50) PRIMARY KEY,
            amountofwins INT NOT NULL
        );

        CREATE TABLE IF NOT EXISTS trade
        (
            tradeID VARCHAR(36) PRIMARY KEY,
            cardID VARCHAR(36) NOT NULL,
            username VARCHAR(50) NOT NULL,
            mindamage INT NOT NULL
        );

        CREATE TABLE IF NOT EXISTS package
        (
            id VARCHAR(36) PRIMARY KEY,
            cardAsJson1 VARCHAR(200) NOT NULL,
            cardAsJson2 VARCHAR(200) NOT NULL,
            cardAsJson3 VARCHAR(200) NOT NULL,
            cardAsJson4 VARCHAR(200) NOT NULL,
            cardAsJson5 VARCHAR(200) NOT NULL
        );

        CREATE TABLE IF NOT EXISTS cardcompendium 
        (
            cardID VARCHAR(36) PRIMARY KEY,
            username VARCHAR(50) NOT NULL,
            amount INT NOT NULL
        );

        CREATE TABLE IF NOT EXISTS deck
        (
            username VARCHAR(50) PRIMARY KEY,
            cardID1 VARCHAR(36),
            cardID2 VARCHAR(36),
            cardID3 VARCHAR(36),
            cardID4 VARCHAR(36)
        );

        CREATE TABLE IF NOT EXISTS card
        (
            cardID VARCHAR(36) PRIMARY KEY,
            cardAsJson VARCHAR(200) NOT NULL
        );";
        // had no time left to put all database functions here
    }
}
