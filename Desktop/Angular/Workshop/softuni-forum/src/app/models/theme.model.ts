import { User } from "./user.model";



export interface Theme{
id:string;
userId: User;

themeName:string;
posts:string[];

subscribers: string[];
created_at:Date;
}