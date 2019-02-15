﻿using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using System;
using System.Net;
using System.IO;
using UnityEngine;

public class Client : MonoBehaviour
{
	public string clientName = "Player One";
	public bool isHost;

	private bool socketReady;
	private TcpClient socket;
	private NetworkStream stream;
	private StreamWriter writer;
	private StreamReader reader;

	private List<GameClient> players = new List<GameClient>();

	private void Start()
	{
		DontDestroyOnLoad(gameObject);
	}

	public bool ConnectToServer(int port, string host)
	{
		if (socketReady)
			return false;

		try
		{
			socket = new TcpClient(host, port);
			stream = socket.GetStream();
			writer = new StreamWriter(stream);
			reader = new StreamReader(stream);

			socketReady = true;
		}
		catch (Exception e)
		{
			Debug.Log("Socket Error: " + e.Message);
		}

		return socketReady;
	}

	private void Update()
	{
		if(socketReady)
		{
			if(stream.DataAvailable)
			{
				string data = reader.ReadLine();
				if (data != null)
					OnIncomingData(data);
			}
		}
	}


	//Send messages to server
	public void Send(string data)
	{
		if (!socketReady)
			return;

		writer.WriteLine(data);
		writer.Flush();
	}
	
	//Read messages from server
	private void OnIncomingData(string data)
	{
		Debug.Log("Client: " + data);
		
		string[] aData = data.Split('|');

		switch(aData[0])
		{
			case "SWHO":
				for(int i = 1; i < aData.Length - 1; i++)
				{
					UserConnected(aData[i], false);
				}
				Send("CWHO|" + clientName + "|" + ((isHost)?1:0).ToString());
				break;
			case "SCNN":
				UserConnected(aData[1], false);
				break;
			case "SMOV":
				CheckersBoard.Instance.TryMove(int.Parse(aData[1]), int.Parse(aData[2]), int.Parse(aData[3]), int.Parse(aData[4]));
				break;
		}
	}

	private void UserConnected(string name,bool host)
	{
		GameClient c = new GameClient();
		c.name = name;

		players.Add(c);

		if (players.Count == 2)
			GameManager.Instance.StartGame();
	}

	private void OnApplicationQuit()
	{
		CloseSocket();
	}
	private void OnDisable()
	{
		CloseSocket();
	}
	private void CloseSocket()
	{
		if (!socketReady)
			return;

		writer.Close();
		reader.Close();
		socket.Close();
		socketReady = false;
	}
}

public class GameClient
{
	//Variables for connecting players
	public string name;
	public bool isHost;
}
