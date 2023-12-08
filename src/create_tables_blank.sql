-- Create tables

CREATE TABLE public.actions (
    id uuid DEFAULT uuid_generate_v4() PRIMARY KEY,
    actiontype varchar(250)
);

CREATE TABLE public.battles (
    id uuid DEFAULT uuid_generate_v4() PRIMARY KEY,
    userid uuid,
    opponentid uuid,
    winner uuid,
    isdraw boolean,
    enddatetime timestamp with time zone DEFAULT CURRENT_TIMESTAMP,
    countrounds smallint
);

CREATE TABLE public.cards (
    id uuid DEFAULT uuid_generate_v4() PRIMARY KEY,
    descr text,
    damage smallint,
    type character(30),
    elementtype uuid,
    name varchar(30)
);

CREATE TABLE public.deck (
    id uuid DEFAULT uuid_generate_v4() PRIMARY KEY,
    cardid uuid,
    userid uuid,
    countcardsoftype smallint
);

CREATE TABLE public.elementtypes (
    id uuid DEFAULT uuid_generate_v4() PRIMARY KEY,
    elementname character(50)
);

CREATE TABLE public.users (
    id uuid DEFAULT uuid_generate_v4() PRIMARY KEY,
    name varchar(30) UNIQUE NOT NULL,
    bio varchar(150),
    image varchar(30),
    password varchar(30) NOT NULL,
    coins smallint
);

-- Add foreign key constraints

ALTER TABLE public.deck
    ADD CONSTRAINT fk_cardid_deck FOREIGN KEY (cardid) REFERENCES public.cards(id);

ALTER TABLE public.cards
    ADD CONSTRAINT fk_elementtypes_cards FOREIGN KEY (elementtype) REFERENCES public.elementtypes(id);

ALTER TABLE public.battles
    ADD CONSTRAINT fk_opponentid_battle FOREIGN KEY (opponentid) REFERENCES public.users(id);

ALTER TABLE public.battles
    ADD CONSTRAINT fk_userid_battle FOREIGN KEY (userid) REFERENCES public.users(id);

ALTER TABLE public.deck
    ADD CONSTRAINT fk_userid_deck FOREIGN KEY (userid) REFERENCES public.users(id);
