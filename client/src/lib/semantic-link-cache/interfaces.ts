import {LinkedRepresentation} from "semantic-link";

export type UriList = string | string[];

export type FormType =
    'http://types/text' |
    'http://types/text/password' |
    'http://types/text/email' |
    'http://types/text/check' |
    'http://types/text/date' |
    'http://types/text/datetime' |
    'http://types/text/select';

export interface FormItem {
    readonly type: FormType | string;
    readonly name: string;
    readonly description?: string;
    readonly required?: boolean;
    readonly items?: FormItem[];
}

export interface FormRepresentation extends LinkedRepresentation {
    items: FormItem[]
}

