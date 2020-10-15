// NAtive_Consumer.cpp : This file contains the 'main' function. Program execution begins and ends there.
//

#include <iostream>
#import "D:\viewstore\ofir.shtainfeld.CORP\alexey.jilinsky_RemoteService_W10_2\RemoteService\RemoteService_SRC\COM\bin\Debug\COM.tlb" raw_interface_only //no_namespace
using namespace std;

int main()
{
    HRESULT hr = CoInitialize(NULL);
    COM::IComPtr _com(__uuidof(COM::ComLib));
    std::cout << "Please enter 1 to start support or 2 to stop support!\n";
    int val;
    while (true)
    {
        cin >> val;
        if (val == 1)
        {
            _com->RequestSupport();
        }
        else if (val == 2)
        {
            _com->StopSupport();
        }
        else if (val == 5)
        {
            break;
        }
        else
        {
            std::cout << "Wrong selected value";
        }
    }
   
    

}

// Run program: Ctrl + F5 or Debug > Start Without Debugging menu
// Debug program: F5 or Debug > Start Debugging menu

// Tips for Getting Started: 
//   1. Use the Solution Explorer window to add/manage files
//   2. Use the Team Explorer window to connect to source control
//   3. Use the Output window to see build output and other messages
//   4. Use the Error List window to view errors
//   5. Go to Project > Add New Item to create new code files, or Project > Add Existing Item to add existing code files to the project
//   6. In the future, to open this project again, go to File > Open > Project and select the .sln file
