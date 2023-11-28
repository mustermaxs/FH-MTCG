-- Create tables

CREATE TABLE public.actions (
    actionid serial PRIMARY KEY,
    actiontype varchar(250)
);

CREATE TABLE public.battles (
    battleid serial PRIMARY KEY,
    userid integer,
    opponentid integer,
    winner integer,
    isdraw boolean,
    enddatetime timestamp with time zone DEFAULT CURRENT_TIMESTAMP,
    countrounds smallint
);

CREATE TABLE public.cards (
    cardid serial PRIMARY KEY,
    descr text,
    damage smallint,
    type character(30),
    elementtype integer,
    name varchar(30)
);

CREATE TABLE public.deck (
    deckid serial PRIMARY KEY,
    cardid integer,
    userid integer,
    countcardsoftype smallint
);

CREATE TABLE public.elementtypes (
    elementtypeid serial PRIMARY KEY,
    elementname character(50)
);

CREATE TABLE public.users (
    userid serial PRIMARY KEY,
    username varchar(30) UNIQUE NOT NULL,
    password varchar(30) NOT NULL,
    coins smallint
);

-- Add foreign key constraints

ALTER TABLE public.deck
    ADD CONSTRAINT fk_cardid_deck FOREIGN KEY (cardid) REFERENCES public.cards(cardid);

ALTER TABLE public.cards
    ADD CONSTRAINT fk_elementtypes_cards FOREIGN KEY (elementtype) REFERENCES public.elementtypes(elementtypeid);

ALTER TABLE public.battles
    ADD CONSTRAINT fk_opponentid_battle FOREIGN KEY (opponentid) REFERENCES public.users(userid);

ALTER TABLE public.battles
    ADD CONSTRAINT fk_userid_battle FOREIGN KEY (userid) REFERENCES public.users(userid);

ALTER TABLE public.deck
    ADD CONSTRAINT fk_userid_deck FOREIGN KEY (userid) REFERENCES public.users(userid);
