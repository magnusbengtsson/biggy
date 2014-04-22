﻿using System;
using System.Collections.Generic;
using System.Linq;
using Biggy;
using Biggy.Postgres;
using Xunit;

namespace Tests.Postgres {
  [Trait("PG Document Store", "")]
  public class PostgresDocumentStore {
    string _connectionStringName = "chinookPG";

    PGDocumentStore<ClientDocument> clientDocs;
    PGDocumentStore<MonkeyDocument> monkeyDocs;

    public PostgresDocumentStore() {
      var _cache = new PGCache(_connectionStringName);

      // Build a table to play with from scratch each time:

      // This needs a fix - gotta pass undelimited table name to one, and delimited to the other. FIX ME, DAMMIT!
      if (_cache.TableExists("ClientDocuments")) {
        _cache.DropTable("\"ClientDocuments\"");
      }
      if (_cache.TableExists("MonkeyDocuments")) {
        _cache.DropTable("\"MonkeyDocuments\"");
      }
      clientDocs = new PGDocumentStore<ClientDocument>(_connectionStringName);
      monkeyDocs = new PGDocumentStore<MonkeyDocument>(_connectionStringName);
    }


    [Fact(DisplayName = "Creates a store with a serial PK if one doesn't exist")]
    public void Creates_Document_Table_With_Serial_PK_If_Not_Present() {
      //clientDocs = new PGDocumentStore<ClientDocument>(_connectionStringName);
      var queryable = clientDocs as IQueryableBiggyStore<ClientDocument>;
      Assert.True(queryable.AsQueryable().Count() == 0);
    }


    [Fact(DisplayName = "Creates a store with a string PK if one doesn't exist")]
    public void Creates_Document_Table_With_String_PK_If_Not_Present() {
      //monkeyDocs = new PGDocumentStore<MonkeyDocument>(_connectionStringName);
      var queryable = monkeyDocs as IQueryableBiggyStore<MonkeyDocument>;
      Assert.True(queryable.AsQueryable().Count() == 0);
    }

    [Fact(DisplayName = "Adds a document with a serial PK")]
    public void Adds_Document_With_Serial_PK() {
      var newCustomer = new ClientDocument {
        Email = "rob@tekpub.com",
        FirstName = "Rob",
        LastName = "Conery"
      };

      IBiggyStore<ClientDocument> docStore = clientDocs as IBiggyStore<ClientDocument>;
      docStore.Add(newCustomer);
      docStore.Load();
      Assert.Equal(1, docStore.Load().Count());
    }

    [Fact(DisplayName = "Updates a document with a serial PK")]
    public void Updates_Document_With_Serial_PK() {
      var newCustomer = new ClientDocument {
        Email = "rob@tekpub.com",
        FirstName = "Rob",
        LastName = "Conery"
      };
      var docStore = clientDocs as IUpdateableBiggyStore<ClientDocument>;
      docStore.Add(newCustomer);
      int idToFind = newCustomer.ClientDocumentId;

      // Go find the new record after reloading:
      var updateMe = docStore.Load().FirstOrDefault(cd => cd.ClientDocumentId == idToFind);
      // Update:
      updateMe.FirstName = "Bill";
      docStore.Update(updateMe);
      // Go find the updated record after reloading:
      var updated = docStore.Load().FirstOrDefault(cd => cd.ClientDocumentId == idToFind);
      Assert.True(updated.FirstName == "Bill");
    }


    [Fact(DisplayName = "Deletes a document with a serial PK")]
    public void Deletes_Document_With_Serial_PK() {
      var newCustomer = new ClientDocument {
        Email = "rob@tekpub.com",
        FirstName = "Rob",
        LastName = "Conery"
      };
      var docStore = clientDocs as IUpdateableBiggyStore<ClientDocument>;
      docStore.Add(newCustomer);
      // Count after adding new:
      int initialCount = docStore.Load().Count();
      var removed = docStore.Remove(newCustomer);
      // Count after removing and reloading:
      int finalCount = docStore.Load().Count();
      Assert.True(finalCount < initialCount);
    }


    [Fact(DisplayName = "Bulk-Inserts new records as JSON documents with string key")]
    public void Bulk_Inserts_Documents_With_String_PK() {
      var updateable = monkeyDocs as IUpdateableBiggyStore<MonkeyDocument>;
      int INSERT_QTY = 100;

      var addRange = new List<MonkeyDocument>();
      for (int i = 0; i < INSERT_QTY; i++) {
        addRange.Add(new MonkeyDocument { Name = "MONKEY " + i, Birthday = DateTime.Today, Description = "The Monkey on my back" });
      }

      updateable.Add(addRange);
      var inserted = updateable.Load();
      Assert.True(inserted.Count() == INSERT_QTY);
    }

    [Fact(DisplayName = "Bulk-Inserts new records as JSON documents with serial int key")]
    void Bulk_Inserts_Documents_With_Serial_PK() {
      var updateable = clientDocs as IUpdateableBiggyStore<ClientDocument>;
      int INSERT_QTY = 100;
      var bulkList = new List<ClientDocument>();
      for (int i = 0; i < INSERT_QTY; i++) {
        var newClientDocument = new ClientDocument {
          FirstName = "ClientDocument " + i,
          LastName = "Test",
          Email = "jatten@example.com"
        };
        bulkList.Add(newClientDocument);
      }
      updateable.Add(bulkList);

      var inserted = updateable.Load();
      Assert.True(inserted.Count() == INSERT_QTY);
    }

  }
}
