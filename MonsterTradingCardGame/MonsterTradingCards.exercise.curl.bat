@echo off

REM --------------------------------------------------
REM Monster Trading Cards Game
REM --------------------------------------------------
title Monster Trading Cards Game
echo CURL Testing for Monster Trading Cards Game
echo.

REM --------------------------------------------------
echo 1) Create Users (Registration)
REM Create User
curl -X POST http://localhost:10001/users --header "Content-Type: application/json" -d "{\"Username\":\"kienboec\", \"Password\":\"daniel\"}"
echo.
curl -X POST http://localhost:10001/users --header "Content-Type: application/json" -d "{\"Username\":\"altenhof\", \"Password\":\"markus\"}"
echo.
curl -X POST http://localhost:10001/users --header "Content-Type: application/json" -d "{\"Username\":\"admin\",    \"Password\":\"istrator\"}"
echo.

echo should fail:
curl -X POST http://localhost:10001/users --header "Content-Type: application/json" -d "{\"Username\":\"kienboec\", \"Password\":\"daniel\"}"
echo.
curl -X POST http://localhost:10001/users --header "Content-Type: application/json" -d "{\"Username\":\"kienboec\", \"Password\":\"different\"}"
echo. 
echo.

REM --------------------------------------------------
echo 2) Login Users
curl -X POST http://localhost:10001/sessions --header "Content-Type: application/json" -d "{\"Username\":\"kienboec\", \"Password\":\"daniel\"}"
echo.
curl -X POST http://localhost:10001/sessions --header "Content-Type: application/json" -d "{\"Username\":\"altenhof\", \"Password\":\"markus\"}"
echo.
curl -X POST http://localhost:10001/sessions --header "Content-Type: application/json" -d "{\"Username\":\"admin\",    \"Password\":\"istrator\"}"
echo.

echo should fail:
curl -X POST http://localhost:10001/sessions --header "Content-Type: application/json" -d "{\"Username\":\"kienboec\", \"Password\":\"different\"}"
echo.
echo.

REM --------------------------------------------------
echo 3) create packages (done by "admin")
curl -X POST http://localhost:10001/packages --header "Content-Type: application/json" --header "Authorization: Bearer admin-mtcgToken" -d "[{\"Id\":\"845f0dc7-37d0-426e-994e-43fc3ac83c08\",\"DinoType\":0, \"Name\":\"Waterdino\",\"Description\":\"Description\", \"Damage\": 10,\"ShopCost\":20}, {\"Id\":\"99f8f8dc-e25e-4a95-aa2c-782823f36e2a\",\"DinoType\":2, \"Name\":\"Dragon\",\"Description\":\"Description\", \"Damage\": 50,\"ShopCost\":200}, {\"Id\":\"e85e3976-7c86-4d06-9a80-641c2019a79f\",\"DinoType\":0,\"SpellType\":0, \"Name\":\"WaterSpell\",\"Description\":\"Description\", \"Damage\": 20,\"ShopCost\":20}, {\"Id\":\"1cb6ab86-bdb2-47e5-b6e4-68c5ab389334\",\"DinoType\":0, \"Name\":\"Orkosaurus\",\"Description\":\"Description\", \"Damage\": 45}, {\"Id\":\"dfdd758f-649c-40f9-ba3a-8657f4b3439f\",\"DinoType\":2,\"SpellType\":0, \"Name\":\"FireSpell\",\"Description\":\"Description\",    \"Damage\": 25}]"
echo.																																																																																		 				    
curl -X POST http://localhost:10001/packages --header "Content-Type: application/json" --header "Authorization: Bearer admin-mtcgToken" -d "[{\"Id\":\"644808c2-f87a-4600-b313-122b02322fd5\",\"DinoType\":0, \"Name\":\"Waterdino\",\"Description\":\"Description\", \"Damage\":  9,\"ShopCost\":20}, {\"Id\":\"4a2757d6-b1c3-47ac-b9a3-91deab093531\",\"DinoType\":2, \"Name\":\"Dragon\",\"Description\":\"Description\", \"Damage\": 55,\"ShopCost\":200}, {\"Id\":\"91a6471b-1426-43f6-ad65-6fc473e16f9f\",\"DinoType\":0,\"SpellType\":0, \"Name\":\"WaterSpell\",\"Description\":\"Description\", \"Damage\": 21,\"ShopCost\":20}, {\"Id\":\"4ec8b269-0dfa-4f97-809a-2c63fe2a0025\",\"DinoType\":0, \"Name\":\"Orkosaurus\",\"Description\":\"Description\", \"Damage\": 55}, {\"Id\":\"f8043c23-1534-4487-b66b-238e0c3c39b5\",\"DinoType\":0,\"SpellType\":2,\"TrapTrigger\":1, \"Name\":\"Erusion\",\"Description\":\"Description\", \"Damage\": 28}]"
echo.																																																																																		 				    
curl -X POST http://localhost:10001/packages --header "Content-Type: application/json" --header "Authorization: Bearer admin-mtcgToken" -d "[{\"Id\":\"b017ee50-1c14-44e2-bfd6-2c0c5653a37c\",\"DinoType\":0, \"Name\":\"Waterdino\",\"Description\":\"Description\", \"Damage\": 11,\"ShopCost\":20}, {\"Id\":\"d04b736a-e874-4137-b191-638e0ff3b4e7\",\"DinoType\":2, \"Name\":\"Dragon\",\"Description\":\"Description\", \"Damage\": 70,\"ShopCost\":200}, {\"Id\":\"88221cfe-1f84-41b9-8152-8e36c6a354de\",\"DinoType\":0,\"SpellType\":0, \"Name\":\"WaterSpell\",\"Description\":\"Description\", \"Damage\": 22,\"ShopCost\":20}, {\"Id\":\"1d3f175b-c067-4359-989d-96562bfa382c\",\"DinoType\":0, \"Name\":\"Orkosaurus\",\"Description\":\"Description\", \"Damage\": 40}, {\"Id\":\"171f6076-4eb5-4a7d-b3f2-2d650cc3d237\",\"DinoType\":1,\"SpellType\":1, \"Name\":\"EarthBuffSpell\",\"Description\":\"Description\", \"Damage\": 28, \"buffAmount\": 10}]"
echo.																																																																																		 				    
curl -X POST http://localhost:10001/packages --header "Content-Type: application/json" --header "Authorization: Bearer admin-mtcgToken" -d "[{\"Id\":\"ed1dc1bc-f0aa-4a0c-8d43-1402189b33c8\",\"DinoType\":0, \"Name\":\"Waterdino\",\"Description\":\"Description\", \"Damage\": 10,\"ShopCost\":20}, {\"Id\":\"65ff5f23-1e70-4b79-b3bd-f6eb679dd3b5\",\"DinoType\":2, \"Name\":\"Dragon\",\"Description\":\"Description\", \"Damage\": 50,\"ShopCost\":200}, {\"Id\":\"55ef46c4-016c-4168-bc43-6b9b1e86414f\",\"DinoType\":0,\"SpellType\":0, \"Name\":\"WaterSpell\",\"Description\":\"Description\", \"Damage\": 20,\"ShopCost\":20}, {\"Id\":\"f3fad0f2-a1af-45df-b80d-2e48825773d9\",\"DinoType\":0, \"Name\":\"Orkosaurus\",\"Description\":\"Description\", \"Damage\": 45}, {\"Id\":\"8c20639d-6400-4534-bd0f-ae563f11f57a\",\"DinoType\":3,\"SpellType\":0, \"Name\":\"AirSpell\",\"Description\":\"Description\",   \"Damage\": 25}]"
echo.																																																																																		 				    
curl -X POST http://localhost:10001/packages --header "Content-Type: application/json" --header "Authorization: Bearer admin-mtcgToken" -d "[{\"Id\":\"d7d0cb94-2cbf-4f97-8ccf-9933dc5354b8\",\"DinoType\":0, \"Name\":\"Waterdino\",\"Description\":\"Description\", \"Damage\":  9,\"ShopCost\":20}, {\"Id\":\"44c82fbc-ef6d-44ab-8c7a-9fb19a0e7c6e\",\"DinoType\":2, \"Name\":\"Dragon\",\"Description\":\"Description\", \"Damage\": 55,\"ShopCost\":200}, {\"Id\":\"2c98cd06-518b-464c-b911-8d787216cddd\",\"DinoType\":0,\"SpellType\":0, \"Name\":\"WaterSpell\",\"Description\":\"Description\", \"Damage\": 21,\"ShopCost\":20}, {\"Id\":\"951e886a-0fbf-425d-8df5-af2ee4830d85\",\"DinoType\":0, \"Name\":\"Orkosaurus\",\"Description\":\"Description\", \"Damage\": 55}, {\"Id\":\"dcd93250-25a7-4dca-85da-cad2789f7198\",\"DinoType\":2,\"SpellType\":2,\"TrapTrigger\":0, \"Name\":\"Wasserkocher\",\"Description\":\"Description\", \"Damage\": 35}]"
echo.																																																																																		 				    
curl -X POST http://localhost:10001/packages --header "Content-Type: application/json" --header "Authorization: Bearer admin-mtcgToken" -d "[{\"Id\":\"b2237eca-0271-43bd-87f6-b22f70d42ca4\",\"DinoType\":0, \"Name\":\"Waterdino\",\"Description\":\"Description\", \"Damage\": 11,\"ShopCost\":20}, {\"Id\":\"9e8238a4-8a7a-487f-9f7d-a8c97899eb48\",\"DinoType\":2, \"Name\":\"Dragon\",\"Description\":\"Description\", \"Damage\": 70,\"ShopCost\":200}, {\"Id\":\"d60e23cf-2238-4d49-844f-c7589ee5342e\",\"DinoType\":0,\"SpellType\":0, \"Name\":\"WaterSpell\",\"Description\":\"Description\", \"Damage\": 22,\"ShopCost\":20}, {\"Id\":\"fc305a7a-36f7-4d30-ad27-462ca0445649\",\"DinoType\":0, \"Name\":\"Orkosaurus\",\"Description\":\"Description\", \"Damage\": 40}, {\"Id\":\"84d276ee-21ec-4171-a509-c1b88162831c\",\"DinoType\":3,\"SpellType\":1, \"Name\":\"AirBuffSpell\",\"Description\":\"Description\", \"Damage\": 28, \"buffAmount\": 10}]"
echo.
echo.

REM --------------------------------------------------
echo 4) acquire packages kienboec
curl -X POST http://localhost:10001/transactions/packages --header "Content-Type: application/json" --header "Authorization: Bearer kienboec-mtcgToken" -d ""
echo.
curl -X POST http://localhost:10001/transactions/packages --header "Content-Type: application/json" --header "Authorization: Bearer kienboec-mtcgToken" -d ""
echo.
curl -X POST http://localhost:10001/transactions/packages --header "Content-Type: application/json" --header "Authorization: Bearer kienboec-mtcgToken" -d ""
echo.
curl -X POST http://localhost:10001/transactions/packages --header "Content-Type: application/json" --header "Authorization: Bearer kienboec-mtcgToken" -d ""
echo.
echo should fail (no money):
curl -X POST http://localhost:10001/transactions/packages --header "Authorization: Bearer kienboec-mtcgToken" -d ""
echo.
echo.

REM --------------------------------------------------
echo 5) acquire packages altenhof
curl -X POST http://localhost:10001/transactions/packages --header "Content-Type: application/json" --header "Authorization: Bearer altenhof-mtcgToken" -d ""
echo.
curl -X POST http://localhost:10001/transactions/packages --header "Content-Type: application/json" --header "Authorization: Bearer altenhof-mtcgToken" -d ""
echo.
echo should fail (no package):
curl -X POST http://localhost:10001/transactions/packages --header "Content-Type: application/json" --header "Authorization: Bearer altenhof-mtcgToken" -d ""
echo.
echo.

REM --------------------------------------------------
echo 6) add new packages
curl -X POST http://localhost:10001/packages --header "Content-Type: application/json" --header "Authorization: Bearer admin-mtcgToken" -d "[{\"Id\":\"845f0dc7-37d0-426e-994e-43fc3ac83c08\",\"DinoType\":0, \"Name\":\"Waterdino\",\"Description\":\"Description\", \"Damage\": 10,\"ShopCost\":20}, {\"Id\":\"99f8f8dc-e25e-4a95-aa2c-782823f36e2a\",\"DinoType\":2, \"Name\":\"Dragon\",\"Description\":\"Description\", \"Damage\": 50,\"ShopCost\":200}, {\"Id\":\"e85e3976-7c86-4d06-9a80-641c2019a79f\",\"DinoType\":0,\"SpellType\":0, \"Name\":\"WaterSpell\",\"Description\":\"Description\", \"Damage\": 20,\"ShopCost\":20}, {\"Id\":\"1cb6ab86-bdb2-47e5-b6e4-68c5ab389334\",\"DinoType\":0, \"Name\":\"Orkosaurus\",\"Description\":\"Description\", \"Damage\": 45}, {\"Id\":\"dfdd758f-649c-40f9-ba3a-8657f4b3439f\",\"DinoType\":2,\"SpellType\":0, \"Name\":\"FireSpell\",\"Description\":\"Description\",    \"Damage\": 25}]"
echo.
curl -X POST http://localhost:10001/packages --header "Content-Type: application/json" --header "Authorization: Bearer admin-mtcgToken" -d "[{\"Id\":\"845f0dc7-37d0-426e-994e-43fc3ac83c08\",\"DinoType\":0, \"Name\":\"Waterdino\",\"Description\":\"Description\", \"Damage\": 10,\"ShopCost\":20}, {\"Id\":\"99f8f8dc-e25e-4a95-aa2c-782823f36e2a\",\"DinoType\":2, \"Name\":\"Dragon\",\"Description\":\"Description\", \"Damage\": 50,\"ShopCost\":200}, {\"Id\":\"e85e3976-7c86-4d06-9a80-641c2019a79f\",\"DinoType\":0,\"SpellType\":0, \"Name\":\"WaterSpell\",\"Description\":\"Description\", \"Damage\": 20,\"ShopCost\":20}, {\"Id\":\"1cb6ab86-bdb2-47e5-b6e4-68c5ab389334\",\"DinoType\":0, \"Name\":\"Orkosaurus\",\"Description\":\"Description\", \"Damage\": 45}, {\"Id\":\"dfdd758f-649c-40f9-ba3a-8657f4b3439f\",\"DinoType\":2,\"SpellType\":0, \"Name\":\"FireSpell\",\"Description\":\"Description\",    \"Damage\": 25}]"
echo.
curl -X POST http://localhost:10001/packages --header "Content-Type: application/json" --header "Authorization: Bearer admin-mtcgToken" -d "[{\"Id\":\"845f0dc7-37d0-426e-994e-43fc3ac83c08\",\"DinoType\":0, \"Name\":\"Waterdino\",\"Description\":\"Description\", \"Damage\": 10,\"ShopCost\":20}, {\"Id\":\"99f8f8dc-e25e-4a95-aa2c-782823f36e2a\",\"DinoType\":2, \"Name\":\"Dragon\",\"Description\":\"Description\", \"Damage\": 50,\"ShopCost\":200}, {\"Id\":\"e85e3976-7c86-4d06-9a80-641c2019a79f\",\"DinoType\":0,\"SpellType\":0, \"Name\":\"WaterSpell\",\"Description\":\"Description\", \"Damage\": 20,\"ShopCost\":20}, {\"Id\":\"1cb6ab86-bdb2-47e5-b6e4-68c5ab389334\",\"DinoType\":0, \"Name\":\"Orkosaurus\",\"Description\":\"Description\", \"Damage\": 45}, {\"Id\":\"dfdd758f-649c-40f9-ba3a-8657f4b3439f\",\"DinoType\":2,\"SpellType\":0, \"Name\":\"FireSpell\",\"Description\":\"Description\",    \"Damage\": 25}]"
echo.
echo.

REM --------------------------------------------------
echo 7) acquire newly created packages altenhof
curl -X POST http://localhost:10001/transactions/packages --header "Content-Type: application/json" --header "Authorization: Bearer altenhof-mtcgToken" -d ""
echo.
curl -X POST http://localhost:10001/transactions/packages --header "Content-Type: application/json" --header "Authorization: Bearer altenhof-mtcgToken" -d ""
echo.
echo should fail (no money):
curl -X POST http://localhost:10001/transactions/packages --header "Content-Type: application/json" --header "Authorization: Bearer altenhof-mtcgToken" -d ""
echo.
echo.

REM --------------------------------------------------
echo 8) show all acquired cards kienboec
curl -X GET http://localhost:10001/cards --header "Authorization: Bearer kienboec-mtcgToken"
echo should fail (no token)
curl -X GET http://localhost:10001/cards 
echo.
echo.

REM --------------------------------------------------
echo 9) show all acquired cards altenhof
curl -X GET http://localhost:10001/cards --header "Authorization: Bearer altenhof-mtcgToken"
echo.
echo.

REM --------------------------------------------------
echo 10) show unconfigured deck
curl -X GET http://localhost:10001/deck --header "Authorization: Bearer kienboec-mtcgToken"
echo.
curl -X GET http://localhost:10001/deck --header "Authorization: Bearer altenhof-mtcgToken"
echo.
echo.

REM --------------------------------------------------
echo 11) configure deck
curl -X PUT http://localhost:10001/deck --header "Content-Type: application/json" --header "Authorization: Bearer kienboec-mtcgToken" -d "[\"845f0dc7-37d0-426e-994e-43fc3ac83c08\", \"99f8f8dc-e25e-4a95-aa2c-782823f36e2a\", \"f8043c23-1534-4487-b66b-238e0c3c39b5\", \"171f6076-4eb5-4a7d-b3f2-2d650cc3d237\"]"
echo.
curl -X GET http://localhost:10001/deck --header "Authorization: Bearer kienboec-mtcgToken"
echo.
curl -X PUT http://localhost:10001/deck --header "Content-Type: application/json" --header "Authorization: Bearer altenhof-mtcgToken" -d "[\"1cb6ab86-bdb2-47e5-b6e4-68c5ab389334\", \"91a6471b-1426-43f6-ad65-6fc473e16f9f\", \"d60e23cf-2238-4d49-844f-c7589ee5342e\", \"84d276ee-21ec-4171-a509-c1b88162831c\"]"
echo.
curl -X GET http://localhost:10001/deck --header "Authorization: Bearer altenhof-mtcgToken"
echo.
echo.
echo should fail and show original from before:
curl -X PUT http://localhost:10001/deck --header "Content-Type: application/json" --header "Authorization: Bearer altenhof-mtcgToken" -d "[\"845f0dc7-37d0-426e-994e-43fc3ac83c08\", \"99f8f8dc-e25e-4a95-aa2c-782823f36e2a\", \"e85e3976-7c86-4d06-9a80-641c2019a79f\", \"171f6076-4eb5-4a7d-b3f2-2d650cc3d237\"]"
echo.
curl -X GET http://localhost:10001/deck --header "Authorization: Bearer altenhof-mtcgToken"
echo.
echo.
echo should fail ... only 3 cards set
curl -X PUT http://localhost:10001/deck --header "Content-Type: application/json" --header "Authorization: Bearer altenhof-mtcgToken" -d "[\"aa9999a0-734c-49c6-8f4a-651864b14e62\", \"d6e9c720-9b5a-40c7-a6b2-bc34752e3463\", \"d60e23cf-2238-4d49-844f-c7589ee5342e\"]"
echo.


REM --------------------------------------------------
echo 12) show configured deck 
curl -X GET http://localhost:10001/deck --header "Authorization: Bearer kienboec-mtcgToken"
echo.
curl -X GET http://localhost:10001/deck --header "Authorization: Bearer altenhof-mtcgToken"
echo.
echo.

REM --------------------------------------------------
echo 13) show configured deck different repentation
echo kienboec
curl -X GET http://localhost:10001/deck?format=plain --header "Authorization: Bearer kienboec-mtcgToken"
echo.
echo.
echo altenhof
curl -X GET http://localhost:10001/deck?format=plain --header "Authorization: Bearer altenhof-mtcgToken"
echo.
echo.

REM --------------------------------------------------
echo 14) edit user data
echo.
curl -X GET http://localhost:10001/users/kienboec --header "Authorization: Bearer kienboec-mtcgToken"
echo.
curl -X GET http://localhost:10001/users/altenhof --header "Authorization: Bearer altenhof-mtcgToken"
echo.
curl -X PUT http://localhost:10001/users/kienboec --header "Content-Type: application/json" --header "Authorization: Bearer kienboec-mtcgToken" -d "{\"Name\": \"Kienboeck\",  \"Bio\": \"me playin...\", \"Image\": \":-)\"}"
echo.
curl -X PUT http://localhost:10001/users/altenhof --header "Content-Type: application/json" --header "Authorization: Bearer altenhof-mtcgToken" -d "{\"Name\": \"Altenhofer\", \"Bio\": \"me codin...\",  \"Image\": \":-D\"}"
echo.
curl -X GET http://localhost:10001/users/Kienboeck --header "Authorization: Bearer Kienboeck-mtcgToken"
echo.
curl -X GET http://localhost:10001/users/Altenhofer --header "Authorization: Bearer Altenhofer-mtcgToken"
echo.
echo.
echo should fail:
curl -X GET http://localhost:10001/users/Altenhofer --header "Authorization: Bearer Kienboeck-mtcgToken"
echo.
curl -X GET http://localhost:10001/users/Kienboeck --header "Authorization: Bearer Altenhofer-mtcgToken"
echo.
curl -X PUT http://localhost:10001/users/Kienboeck --header "Content-Type: application/json" --header "Authorization: Bearer Altenhofer-mtcgToken" -d "{\"Name\": \"Hoax\",  \"Bio\": \"me playin...\", \"Image\": \":-)\"}"
echo.
curl -X PUT http://localhost:10001/users/Altenhofer --header "Content-Type: application/json" --header "Authorization: Bearer Kienboeck-mtcgToken" -d "{\"Name\": \"Hoax\", \"Bio\": \"me codin...\",  \"Image\": \":-D\"}"
echo.
curl -X GET http://localhost:10001/users/someGuy  --header "Authorization: Bearer Altenhofer-mtcgToken"
echo.
echo.

REM --------------------------------------------------
echo 15) stats
curl -X GET http://localhost:10001/stats --header "Authorization: Bearer Kienboeck-mtcgToken"
echo.
curl -X GET http://localhost:10001/stats --header "Authorization: Bearer Altenhofer-mtcgToken"
echo.
echo.

REM --------------------------------------------------
echo 16) scoreboard
curl -X GET http://localhost:10001/scoreboard --header "Authorization: Bearer Kienboeck-mtcgToken"
echo.
echo.

REM --------------------------------------------------
echo 17) battle
start /b "Kienboeck battle" curl -X POST http://localhost:10001/battles --header "Authorization: Bearer Kienboeck-mtcgToken"
start /b "Altenhofer battle" curl -X POST http://localhost:10001/battles --header "Authorization: Bearer Altenhofer-mtcgToken"
ping localhost -n 10 >NUL 2>NUL

REM --------------------------------------------------
echo 18) Stats 
echo kienboec
curl -X GET http://localhost:10001/stats --header "Authorization: Bearer Kienboeck-mtcgToken"
echo.
echo altenhof
curl -X GET http://localhost:10001/stats --header "Authorization: Bearer Altenhofer-mtcgToken"
echo.
echo.

REM --------------------------------------------------
echo 19) scoreboard
curl -X GET http://localhost:10001/scoreboard --header "Authorization: Bearer Kienboeck-mtcgToken"
echo.
echo.

REM --------------------------------------------------
echo 20) trade
echo check trading deals
curl -X GET http://localhost:10001/tradings --header "Authorization: Bearer Kienboeck-mtcgToken"
echo.
echo create trading deal
curl -X POST http://localhost:10001/tradings --header "Content-Type: application/json" --header "Authorization: Bearer Kienboeck-mtcgToken" -d "{\"tradeID\": \"6cd85277-4590-49d4-b0cf-ba0a921faad0\", \"CardToTrade\": \"1cb6ab86-bdb2-47e5-b6e4-68c5ab389334\", \"minDamage\": 15}"
echo.
echo check trading deals
curl -X GET http://localhost:10001/tradings --header "Authorization: Bearer Kienboeck-mtcgToken"
echo.
curl -X GET http://localhost:10001/tradings --header "Authorization: Bearer Altenhofer-mtcgToken"
echo.
echo delete trading deals
curl -X DELETE http://localhost:10001/tradings/6cd85277-4590-49d4-b0cf-ba0a921faad0 --header "Authorization: Bearer Kienboeck-mtcgToken"
echo.
echo.

REM --------------------------------------------------
echo 21) check trading deals
curl -X GET http://localhost:10001/tradings  --header "Authorization: Bearer Kienboeck-mtcgToken"
echo.
curl -X POST http://localhost:10001/tradings --header "Content-Type: application/json" --header "Authorization: Bearer Kienboeck-mtcgToken" -d "{\"tradeID\": \"6cd85277-4590-49d4-b0cf-ba0a921faad0\", \"CardToTrade\": \"1cb6ab86-bdb2-47e5-b6e4-68c5ab389334\", \"minDamage\": 15}"
echo check trading deals
curl -X GET http://localhost:10001/tradings  --header "Authorization: Bearer Kienboeck-mtcgToken"
echo.
curl -X GET http://localhost:10001/tradings  --header "Authorization: Bearer Altenhofer-mtcgToken"
echo.
echo try to trade with yourself (should fail)
curl -X POST http://localhost:10001/tradings/6cd85277-4590-49d4-b0cf-ba0a921faad0 --header "Content-Type: application/json" --header "Authorization: Bearer Kienboeck-mtcgToken" -d "{\"tradeID\": \"4ec8b269-0dfa-4f97-809a-2c63fe2a0025\"}"
echo.
echo try to trade 
echo.
curl -X POST http://localhost:10001/tradings/6cd85277-4590-49d4-b0cf-ba0a921faad0 --header "Content-Type: application/json" --header "Authorization: Bearer Altenhofer-mtcgToken" -d "{\"tradeID\": \"951e886a-0fbf-425d-8df5-af2ee4830d85\"}"
echo.
curl -X GET http://localhost:10001/tradings --header "Authorization: Bearer Kienboeck-mtcgToken"
echo.
curl -X GET http://localhost:10001/tradings --header "Authorization: Bearer Altenhofer-mtcgToken"
echo.

REM --------------------------------------------------
echo end...

REM this is approx a sleep 
ping localhost -n 100 >NUL 2>NUL
@echo on
