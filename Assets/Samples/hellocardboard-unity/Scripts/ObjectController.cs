//-----------------------------------------------------------------------
// <copyright file="ObjectController.cs" company="Google LLC">
// Copyright 2020 Google LLC
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//-----------------------------------------------------------------------

using System.Collections;
using UnityEngine;

/// <summary>
/// Controls target objects behaviour.
/// </summary>
public class ObjectController : MonoBehaviour
{
    /// <summary>
    /// The material to use when this object is inactive (not being gazed at).
    /// </summary>
    public Material InactiveMaterial;

    /// <summary>
    /// The material to use when this object is active (gazed at).
    /// </summary>
    public Material GazedAtMaterial;

    // The objects are about 1 meter in radius, so the min/max target distance are
    // set so that the objects are always within the room (which is about 5 meters
    // across).
    private const float _minObjectDistance = 2.5f;
    private const float _maxObjectDistance = 3.5f;
    private const float _minObjectHeight = 0.5f;
    private const float _maxObjectHeight = 3.5f;

    private Renderer _myRenderer;
    private Vector3 _startingPosition;

    // <symmary>
    // Variables de clase para generar INTERACTIVIDAD a travess de la vista y generar teletransportacion entre mundos
    // </symmary>
    private float tiempoADecrementar = 3.0f;
    private float tiempoInicial, tiempoActual;
    private int status = 0;
    public GameObject playerVR;

    [Header("Configuración de Audio")]
    public AudioSource sourceAmbiente; // Arrastra aquí el AudioSource de tu Player
    public AudioClip musicaMundo1; // Audio para coordenadas (0, 5, 0)
    public AudioClip musicaMundo2; // Audio para coordenadas (0, 5, 30)
    public AudioClip musicaMundo3; // Audio para coordenadas (0, 5, 60)

    private static float tiempoUltimoSalto = 0f;
    private float duracionEnfriamiento = 3.0f;


    [Header("Interfaz de Usuario")]
    public GameObject canvasIntro;

    /// <summary>
    /// Start is called before the first frame update.
    /// </summary>
    public void Start()
    {
        _startingPosition = transform.parent.localPosition;
        _myRenderer = GetComponent<Renderer>();
        SetMaterial(false);
    }

    /// <summary>
    /// Teleports this instance randomly when triggered by a pointer click.
    /// </summary>
    public void TeleportRandomly()
    {
        // Picks a random sibling, activates it and deactivates itself.
        int sibIdx = transform.GetSiblingIndex();
        int numSibs = transform.parent.childCount;
        sibIdx = (sibIdx + Random.Range(1, numSibs)) % numSibs;
        GameObject randomSib = transform.parent.GetChild(sibIdx).gameObject;

        // Computes new object's location.
        float angle = Random.Range(-Mathf.PI, Mathf.PI);
        float distance = Random.Range(_minObjectDistance, _maxObjectDistance);
        float height = Random.Range(_minObjectHeight, _maxObjectHeight);
        Vector3 newPos = new Vector3(Mathf.Cos(angle) * distance, height,
                                     Mathf.Sin(angle) * distance);

        // Moves the parent to the new position (siblings relative distance from their parent is 0).
        transform.parent.localPosition = newPos;

        randomSib.SetActive(true);
        gameObject.SetActive(false);
        SetMaterial(false);
    }

    public void Update()
    {
        // <symmary>
        // Lógica para teletransportacion entre mundos
        // </symmary>
        if (status == 1)
        {

            if (Time.time < tiempoUltimoSalto + duracionEnfriamiento)
            {
                return;
            }

            if (tiempoActual >= 0)
            {
                tiempoActual = tiempoADecrementar - (Time.time - tiempoInicial);

                GameObject textoObj = GameObject.Find("GraphicsAPIText");
                if (textoObj != null)
                    textoObj.GetComponent<GraphicsAPITextController>().Escribe(tiempoActual.ToString("F2"));
            } else
            {

                tiempoUltimoSalto = Time.time;

                // LÓGICA DE TELETRANSPORTACIÓN Y AUDIO
                switch (CardboardReticlePointer.nombrePortal)
                {
                    // Viajando HACIA el Mundo 2 (z = 30)
                    case "Portal12":
                        playerVR.transform.position = new Vector3(0, 5f, 30);
                        CambiarMusica(musicaMundo2);
                        if (canvasIntro != null) canvasIntro.SetActive(false);
                        break;

                    // Viajando HACIA el Mundo 3 (z = 60)
                    case "Portal13":
                        playerVR.transform.position = new Vector3(0, 5f, 60);
                        CambiarMusica(musicaMundo3);
                        if (canvasIntro != null) canvasIntro.SetActive(false);
                        break;

                    // Viajando HACIA el Mundo 1 (z = 0)
                    case "Portal21":
                        playerVR.transform.position = new Vector3(0, 5f, 0);
                        CambiarMusica(musicaMundo1);
                        if (canvasIntro != null) canvasIntro.SetActive(true);
                        break;

                    // Viajando HACIA el Mundo 3 (z = 60)
                    case "Portal23":
                        playerVR.transform.position = new Vector3(0, 5f, 60);
                        CambiarMusica(musicaMundo3);
                        if (canvasIntro != null) canvasIntro.SetActive(true);
                        break;

                    // Viajando HACIA el Mundo 1 (z = 0)
                    case "Portal31":
                        playerVR.transform.position = new Vector3(0, 5f, 0);
                        CambiarMusica(musicaMundo1);
                        if (canvasIntro != null) canvasIntro.SetActive(false);
                        break;

                    // Viajando HACIA el Mundo 2 (z = 30)
                    case "Portal32":
                        playerVR.transform.position = new Vector3(0, 5f, 30);
                        if (canvasIntro != null) canvasIntro.SetActive(false);
                        CambiarMusica(musicaMundo2);
                        break;

                    default:
                        break;
                }
                // Reseteamos el status para que no siga ejecutando el switch infinitamente
                status = 0;
                OnPointerExit(); // Forzamos la salida para limpiar el estado visual
            }
        }
    }

    void MoverJugador(float x, float y, float z, AudioClip musica, bool mostrarCanvas)
    {
        playerVR.transform.position = new Vector3(x, y, z);
        CambiarMusica(musica);
        if (canvasIntro != null) canvasIntro.SetActive(mostrarCanvas);
    }

    void CambiarMusica(AudioClip nuevaMusica)
    {
        // Solo cambiamos si la canción es diferente a la que ya suena
        if (sourceAmbiente.clip != nuevaMusica)
        {
            sourceAmbiente.Stop();
            sourceAmbiente.clip = nuevaMusica;
            sourceAmbiente.Play();
        }
    }

    /// <summary>
    /// This method is called by the Main Camera when it starts gazing at this GameObject.
    /// </summary>
    public void OnPointerEnter()
    {
        if (Time.time < tiempoUltimoSalto + duracionEnfriamiento)
        {
            return;
        }

        SetMaterial(true);
        status = 1;
        tiempoInicial = Time.time;
        tiempoActual = tiempoADecrementar;
    }

    /// <summary>
    /// This method is called by the Main Camera when it stops gazing at this GameObject.
    /// </summary>
    public void OnPointerExit()
    {
        SetMaterial(false);
        status = 0;
        //GameObject.Find("GraphicsAPIText").GetComponent<GraphicsAPITextController>().Escribe("T final: " + tiempoActual.ToString("F2"));
    }

    /// <summary>
    /// This method is called by the Main Camera when it is gazing at this GameObject and the screen
    /// is touched.
    /// </summary>
    public void OnPointerClick()
    {
        //TeleportRandomly();
    }

    /// <summary>
    /// Sets this instance's material according to gazedAt status.
    /// </summary>
    ///
    /// <param name="gazedAt">
    /// Value `true` if this object is being gazed at, `false` otherwise.
    /// </param>
    private void SetMaterial(bool gazedAt)
    {
        if (InactiveMaterial != null && GazedAtMaterial != null)
        {
            _myRenderer.material = gazedAt ? GazedAtMaterial : InactiveMaterial;
        }
    }
}
