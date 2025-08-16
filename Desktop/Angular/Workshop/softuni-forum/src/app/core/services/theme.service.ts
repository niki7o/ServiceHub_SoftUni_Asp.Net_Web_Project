import { HttpClient } from "@angular/common/http";
import { Injectable } from "@angular/core";
import { Observable } from "rxjs";
import { Theme } from "../../models/theme.model";


@Injectable({
    providedIn: 'root' 
})
export class ThemeService {
    private apiUrl = 'http://localhost:3000/api/themes';

    constructor(private httpClient: HttpClient) {}

    getThemes(): Observable<Theme[]> {
        return this.httpClient.get<Theme[]>(this.apiUrl);
    }
}