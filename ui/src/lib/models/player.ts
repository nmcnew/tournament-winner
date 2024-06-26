import type Game from './repo/Game';
import type Community from './repo/Community';
import type { GameCharacter } from './repo/Game';

export interface Player {
    id: string;
    prefix?: string;
    userId?: string;
    user?: User;
    name: string;
    playerCreationDate: Date;
    previousNames: Array<string>;
    games: Array<PlayerGame>;
}

export interface PlayerGame {
    game: Game;
    player: Player;
    characters: Array<GameCharacter>;
}

export interface User {
    id: string;
    name: string;
    email: string;
    userCreationDate: Date;
    insertDate: Date;
    profile?: Profile;
}

export interface Profile {
    id: string;
    prefix: string;
    handle: string;
    firstName: string;
    lastName: string;
    profileImage: string;
    user?: User;
    insertDate: Date;
}
export interface ExternalLinks {
    icon: string;
    text: string;
    url: string;
}
export interface ContactMethod {
    type: string; //eg Phone Number, Discord, etc.
    value: string;
}
export interface UserAuthMethod {
    id: number;
    userId: string;
    authValue: string;
    authProviderId: string;
}
export interface CommunityGame {
    id: number;
    community: Community;
    owner: User;
    game: Game;
    insertDate: Date;
}
