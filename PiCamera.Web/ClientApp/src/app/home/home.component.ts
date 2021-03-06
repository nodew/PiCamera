import { Component, OnInit, ElementRef } from '@angular/core';
import * as SignalR from '@microsoft/signalr';
import * as JMuxer from "jmuxer";
import { CameraService } from "../camera.service";

@Component({
  selector: 'app-home',
  templateUrl: './home.component.html'
})
export class HomeComponent implements OnInit {
  jmuxer: JMuxer;

  constructor(
    private el: ElementRef,
    private cameraService: CameraService
  ) {}

  async ngOnInit() {
    this.jmuxer = new JMuxer({
      node: this.el.nativeElement.querySelector("#player"),
      mode: "video",
      fps: 30,
      debug: true
    });

    let connection = new SignalR.HubConnectionBuilder()
      .withUrl("/videoStream")
      .build();

    connection.on("ReceiveFragment", data => {
      if (data && data.fragment) {
        const arrayData = this.fromBase64ToUint8Array(data.fragment);
        this.jmuxer.feed({
          video: arrayData,
        });
      }
    });

    await connection.start();

  }

  start() {
    console.log("Starting")
    this.cameraService.start().subscribe(() => {
      console.log("Started");
    });
  }

  stop() {
    console.log("Stopping")
    this.cameraService.stop().subscribe(() => {
      console.log("Stopped");
    });
  }

  private fromBase64ToUint8Array(base64: string) {
    var binaryString = window.atob(base64);
    var len = binaryString.length;
    var bytes = new Uint8Array(len);
    for (var i = 0; i < len; i++) {
      bytes[i] = binaryString.charCodeAt(i);
    }
    return bytes;
  }
}
