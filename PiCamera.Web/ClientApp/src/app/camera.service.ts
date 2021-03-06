import { Injectable } from "@angular/core";
import { HttpClient } from "@angular/common/http";

@Injectable({
  providedIn: 'root',
})
export class CameraService {
  constructor(private httpClient: HttpClient) {}

  start() {
    return this.httpClient.post("/Camera/Start", null);
  }

  stop() {
    return this.httpClient.post("/Camera/Stop", null);
  }
}
