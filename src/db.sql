CREATE TABLE IF NOT EXISTS users (
    userId SERIAL PRIMARY KEY NOT NULL,
    username VARCHAR(30) UNIQUE NOT NULL,
    password VARCHAR(30) NOT NULL,
    coins SMALLINT
);
CREATE TABLE IF NOT EXISTS cards (
    cardId SERIAL PRIMARY KEY NOT NULL,
    descr TEXT,
    damage SMALLINT,
    elementType INT,
    type CHAR(30),
    CONSTRAINT fk_elementtypes_cards
        FOREIGN KEY(elementType)
            REFERENCES elementtypes(elementTyepId)
);
CREATE TABLE IF NOT EXISTS deck (
    deckId SERIAL PRIMARY KEY NOT NULL,
    cardId INT,
    userId INT,
    CONSTRAINT fk_userId_deck FOREIGN KEY(userId) REFERENCES users(userId),
    CONSTRAINT fk_cardId_deck FOREIGN KEY(cardId) REFERENCES cards(cardId)
);
CREATE TABLE IF NOT EXISTS battles (
    battleId SERIAL PRIMARY KEY NOT NULL,
    userId INT,
    opponentId INT,
    winner INT,
    isDraw BOOLEAN,
    endDateTime TIMESTAMPTZ DEFAULT CURRENT_TIMESTAMP,
    countRounds SMALLINT,
    CONSTRAINT fk_userId_battle FOREIGN KEY(userId) REFERENCES users(userId),
    CONSTRAINT fk_opponentId_battle FOREIGN KEY(opponentId) REFERENCES users(userId)
);
CREATE TABLE IF NOT EXISTS actions (
    actionId SERIAL PRIMARY KEY NOT NULL,
    actionType VARCHAR(250)
);
CREATE TABLE IF NOT EXISTS elementtypes (
    elementTyepId SERIAL PRIMARY KEY NOT NULL,
    elementName CHAR(50)
);